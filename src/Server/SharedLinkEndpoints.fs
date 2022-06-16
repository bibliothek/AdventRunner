module SharedLinkEndpoints

open System
open FSharp.Control.Tasks
open Microsoft.AspNetCore.Http
open Giraffe
open EndpointsHelpers
open Shared
open Storage

let getHandler (id: string) next (ctx: HttpContext) =
    let linkStorage = ctx.GetService<SharedLinksStorage>()
    let userDataStorage = ctx.GetService<UserDataStorage>()

    let sharedLink = linkStorage.GetSharedLink id

    let userData =
        userDataStorage.GetUserData sharedLink.owner

    let cal = userData.calendars.[sharedLink.period]

    json cal next ctx

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
    let linkStorage = ctx.GetService<SharedLinksStorage>()
    let userDataStorage = ctx.GetService<UserDataStorage>()
    let owner = getUser ctx

    let sharedLink = linkStorage.GetSharedLink id
    let userData = userDataStorage.GetUserData owner

    let updatedUserData =
        getUpdatedSharedLinkInUserData userData None sharedLink.period

    userDataStorage.UpdateUserData updatedUserData
    |> ignore

    linkStorage.DeleteSharedLink id |> ignore
    ctx.SetStatusCode 204
    next ctx

type SharedLinkPostRequest = { period: int }

let postHandler next (ctx: HttpContext) =
    task {
        let linkStorage = ctx.GetService<SharedLinksStorage>()
        let userDataStorage = ctx.GetService<UserDataStorage>()
        let owner = getUser ctx

        let! sharedLinkPostRequest = ctx.BindJsonAsync<SharedLinkPostRequest>()
        let sharedLinkId = (Guid.NewGuid() |> ShortGuid.fromGuid)

        let sharedLink =
            { id = sharedLinkId
              owner = owner
              period = sharedLinkPostRequest.period }

        let userData = userDataStorage.GetUserData owner

        let updatedUserData =
            getUpdatedSharedLinkInUserData userData (Some sharedLinkId) sharedLinkPostRequest.period

        userDataStorage.UpdateUserData updatedUserData
        |> ignore

        return! json (linkStorage.UpsertSharedLink sharedLink) next ctx
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
