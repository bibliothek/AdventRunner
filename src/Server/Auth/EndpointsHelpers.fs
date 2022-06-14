module EndpointsHelpers

open Microsoft.AspNetCore.Http
open Giraffe

let notLoggedIn =
    setStatusCode 403 >=> text "Forbidden"

let mustBeLoggedIn : (HttpFunc -> HttpContext -> HttpFuncResult)= requiresAuthentication notLoggedIn

