module Server.CalendarEndpoints

open Microsoft.AspNetCore.Http
open Giraffe
open Newtonsoft.Json
open Server.MsgProcessor
open Shared
open Server.Storage
open Server.Auth.EndpointsHelpers

let migrate (storage: UserDataStorage) (userData: UserData) =
    let period = Calendar.currentPeriod ()

    if userData.version <> "2.0" then
        let userData = Calendar.initUserData userData.owner Settings.initDefault

        Some(storage.AddNewUser userData)
    elif userData.latestPeriod <> period then
        let _, previousCalendar =
            userData.calendars |> Map.toList |> List.sortByDescending fst |> List.head

        let newCalendar = Calendar.initCalendar Settings.initDefault

        let userDataWithNewPeriod =
            { userData with
                latestPeriod = period
                calendars =
                    userData.calendars.Add(
                        period,
                        { newCalendar with
                            settings = previousCalendar.settings }
                    ) }

        Some(storage.UpdateUserData userDataWithNewPeriod)
    else
        None

let getHandler next (ctx: HttpContext) =
    let owner = getUser ctx
    let storage = ctx.GetService<UserDataStorage>()

    let userData =
        match (storage.UserExists owner) with
        | true ->
            let cal = storage.GetUserData owner
            migrate storage cal |> Option.defaultValue cal
        | false -> storage.AddNewUser(Calendar.initUserData owner Settings.initDefault)

    if owner |> Auth0Client.canVerifyDistance then
        let queue = ctx.GetService<SyncQueue>()
        queue.Enqueue {owner = owner }

    json userData next ctx


let putHandler next (ctx: HttpContext) =
    task {
        let owner = getUser ctx
        let! requestBody = ctx.ReadBodyFromRequestAsync ()
        let userData = JsonConvert.DeserializeObject<UserData> requestBody
        let userDataWithOwner = { userData with owner = owner }
        let storage = ctx.GetService<UserDataStorage>()
        return! json (storage.UpdateUserData userDataWithOwner) next ctx
    }

let postHandler next (ctx: HttpContext) =
    let owner = getUser ctx
    let storage = ctx.GetService<UserDataStorage>()
    let linkStorage = ctx.GetService<SharedLinksStorage>()
    let oldCal = storage.GetUserData owner

    let sharedLinks =
        oldCal.calendars
        |> Map.toList
        |> List.map (fun x -> (x |> snd).settings.sharedLinkId)
        |> List.choose id

    sharedLinks |> List.iter (fun x -> linkStorage.DeleteSharedLink x |> ignore)

    let cal = Calendar.initUserData owner Settings.initDefault

    json (storage.AddNewUser cal) next ctx

let handlers: HttpFunc -> HttpContext -> HttpFuncResult =
    mustBeLoggedIn
    >=> choose
            [ GET >=> route "/api/calendars" >=> getHandler
              PUT >=> route "/api/calendars" >=> putHandler
              POST >=> route "/api/calendars" >=> postHandler ]
