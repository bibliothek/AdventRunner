module Server.StravaSync

open System
open System.Threading.Tasks

// we cannot filter by local time in Strava
// instead we include more elements and then filter the results (which do contain the local time)
let private getDecemberTimestamps (period: int) =
    let beginning =
        DateTimeOffset(period, 11, 30, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds()

    let ending =
        DateTimeOffset(period + 1, 1, 2, 0, 0, 0, TimeSpan.Zero).ToUnixTimeSeconds()

    (beginning, ending)

let private getActivities owner (period: int64*int64) =
    task {
        let! auth0Token = Auth0Client.getManagementAccessTokenAsync
        let! refreshToken = Auth0Client.getStravaRefreshToken auth0Token owner

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

let getTotalDistance owner (period: int) =
    let timestamps = getDecemberTimestamps period
    if fst timestamps < DateTimeOffset.UtcNow.ToUnixTimeSeconds() then
        task {
            let! activities = getActivities owner timestamps

            if activities.IsSome then
                return Some(getTotalDistanceFromActivities activities.Value)
            else
                return None
        }
    else Task.FromResult(None)