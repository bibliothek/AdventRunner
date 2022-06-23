module EndpointsHelpers

open System.Security.Claims
open Microsoft.AspNetCore.Http
open Giraffe
open Shared

let notLoggedIn = setStatusCode 403 >=> text "Forbidden"

let mustBeLoggedIn: (HttpFunc -> HttpContext -> HttpFuncResult) = requiresAuthentication notLoggedIn


let getUser (ctx: HttpContext) : Owner =
    { name =
          ctx
              .User
              .FindFirst(
                  ClaimTypes.NameIdentifier
              )
              .Value }
