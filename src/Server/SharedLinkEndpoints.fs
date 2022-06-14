module SharedLinkEndpoints

open Microsoft.AspNetCore.Http
open Giraffe
open EndpointsHelpers

let getHandler (id: string) next (ctx: HttpContext) =
    json id next ctx

let handlers: HttpFunc -> HttpContext -> HttpFuncResult =
    choose [ GET >=> routef "/api/sharedCalendars/%s" getHandler
             mustBeLoggedIn >=> GET >=> routef "/api/sharedCalendarss/%s" getHandler ]
