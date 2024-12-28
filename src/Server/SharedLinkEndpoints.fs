module Server.SharedLinkEndpoints

open System
open Microsoft.AspNetCore.Http
open Giraffe
open Server.Auth.EndpointsHelpers
open Shared
open Server.SqliteStorage

let getHandler (id: string) next (ctx: HttpContext) =
    let userDataStorage = ctx.GetService<UserDataStorage>()
    let userByLink = userDataStorage.GetUserDataBySharedLink id

    match userByLink with
    | None ->
        ctx.SetStatusCode 404
        next ctx
    | Some userData ->
        let response: SharedLinkResponse = { calendar = userData.calendars.Values |> Seq.head ; displayName = userData.displayName; period = userData.calendars.Keys |> Seq.head}
        json response next ctx

let getUpdatedSharedLinkInUserData userData sharedLinkOption period =
    let calendarToUpdate = userData.calendars.[period]

    let updatedCalendar =
        { calendarToUpdate with
              settings =
                  { calendarToUpdate.settings with
                        sharedLinkId = sharedLinkOption } }

    { userData with
          calendars = userData.calendars.Add(period, updatedCalendar) }

let deleteHandler (id: string) next (ctx: HttpContext) =
    let userDataStorage = ctx.GetService<UserDataStorage>()
    let userData = userDataStorage.GetUserDataBySharedLink id

    match userData with
    | None ->
        ctx.SetStatusCode 204
        next ctx
    | Some user ->
        let updatedUserData =
            getUpdatedSharedLinkInUserData user None (user.calendars.Keys |> Seq.head)

        userDataStorage.UpdateUserData updatedUserData |> ignore
        ctx.SetStatusCode 204
        next ctx

let postHandler next (ctx: HttpContext) =
    task {
        let userDataStorage = ctx.GetService<UserDataStorage>()
        let owner = getUser ctx

        let! sharedLinkPostRequest = ctx.BindJsonAsync<Shared.SharedLinkPostRequest>()
        let sharedLinkId = (Guid.NewGuid() |> ShortGuid.fromGuid)

        let userData = userDataStorage.GetUserData owner |> Option.get

        let updatedUserData =
            getUpdatedSharedLinkInUserData userData (Some sharedLinkId) sharedLinkPostRequest.period

        return! json (userDataStorage.UpdateUserData updatedUserData) next ctx
    }


let handlers: HttpFunc -> HttpContext -> HttpFuncResult =
    choose [ GET
             >=> routef "/api/sharedCalendars/%s" getHandler

             mustBeLoggedIn
             >=> POST
             >=> route "/api/sharedCalendars"
             >=> postHandler

             mustBeLoggedIn
             >=> DELETE
             >=> routef "/api/sharedCalendars/%s" deleteHandler ]
