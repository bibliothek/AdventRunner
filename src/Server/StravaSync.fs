module Server.StravaSync

open System
open System.Threading.Tasks
open Microsoft.Extensions.Logging
open Server.Storage
open Shared

type PeriodSelector =
    | All
    | Period of int

// we cannot filter by local time in Strava
// instead we include more elements and then filter the results (which do contain the local time)
let private getDecemberTimestamps (period: int) =
    let beginning =
        DateTimeOffset(period, 11, 30, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds()

    let ending =
        DateTimeOffset(period + 1, 1, 2, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds()

    (beginning, ending)

let private getActivities owner (period: int64 * int64) logger =
    task {
        let! auth0Token = Auth0Client.getManagementAccessTokenAsync logger

        let! refreshToken =
            match auth0Token with
            | Some token -> Auth0Client.getStravaRefreshToken token owner
            | _ -> Task.FromResult None

        if refreshToken.IsSome then
            let! stravaAccessToken = StravaClient.getAccessToken refreshToken.Value
            let! activities = StravaClient.getActivities stravaAccessToken period
            return Some activities
        else
            return None
    }

let private acceptedSportTypes = [ "Run"; "Hike"; "TrailRun"; "VirtualRun"; "Walk" ]

let private getTotalDistanceFromActivities (activities: StravaClient.Activity[]) =
    activities
    |> Array.filter (fun el ->
        acceptedSportTypes |> List.contains el.sport_type
        && (el.start_date_local |> DateTime.Parse).Month = 12)
    |> Array.map (_.distance)
    |> Array.sum

let private getTotalDistance owner (period: int) logger =
    let timestamps = getDecemberTimestamps period

    if fst timestamps < DateTimeOffset.UtcNow.ToUnixTimeSeconds() then
        task {
            let! activities = getActivities owner timestamps logger

            if activities.IsSome then
                return Some(getTotalDistanceFromActivities activities.Value)
            else
                return Some(0.0)
        }
    else
        Task.FromResult(Some(0.0))

let private syncVerifiedDistance (storage: UserDataStorage, (logger: ILogger), (owner: Owner), period) =
    task {
        try
            logger.LogInformation $"Getting distance data for user {owner.name} and period {period}"
            let! totalDistance = getTotalDistance owner period logger
            let userData = storage.GetUserData owner
            logger.LogInformation $"Received distance data for user {owner.name} and period {period}: {totalDistance}"

            if userData.calendars[period].verifiedDistance <> totalDistance then
                logger.LogInformation $"Updating distance data for user {owner.name} and period {period}"

                let calendars =
                    userData.calendars.Change(
                        period,
                        (fun el ->
                            Some
                                { el.Value with
                                    verifiedDistance = totalDistance })
                    )

                storage.UpdateUserData { userData with calendars = calendars } |> ignore
                logger.LogInformation $"Successfully updated distance data for user {owner.name} and period {period}"

            logger.LogInformation $"Finished getting distance data for user {owner.name} and period {period}"
        with ex ->
            logger.LogError(ex, $"Error syncing distance. {ex.Message}")
    }

let sync owner periodSelector (storage: UserDataStorage) (logger: ILogger) =
    task {
        let userData = storage.GetUserData owner

        let periods =
            match periodSelector with
            | All -> userData.calendars.Keys
            | Period p -> [| p |]

        logger.LogInformation $"Syncing user data for user {userData.owner.name} and period {periodSelector}"

        periods
        |> Seq.iter (fun period ->
            syncVerifiedDistance (storage, logger, owner, period)
            |> Async.AwaitTask
            |> Async.RunSynchronously)
    }