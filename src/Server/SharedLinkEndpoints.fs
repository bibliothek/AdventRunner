module SharedLinkEndpoints

open Microsoft.AspNetCore.Http
open Giraffe
open EndpointsHelpers
open Shared
open Storage

let getHandler (id: string) next (ctx: HttpContext) =
    let linkStorage = ctx.GetService<SharedLinksStorage>()
    let userDataStorage = ctx.GetService<UserDataStorage>()

    let sharedLink = linkStorage.GetSharedLink id
    let userData = userDataStorage.GetUserData sharedLink.owner
    let cal = userData.calendars.[sharedLink.period]

    json cal next ctx

let handlers: HttpFunc -> HttpContext -> HttpFuncResult =
    choose [ GET >=> routef "/api/sharedCalendars/%s" getHandler
             mustBeLoggedIn >=> GET >=> routef "/api/sharedCalendarss/%s" getHandler ]
