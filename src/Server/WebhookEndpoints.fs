module Server.WebhookEndpoints

open Microsoft.AspNetCore.Http
open Giraffe

let getHandlerStrava next (ctx: HttpContext) : HttpFuncResult =
    task {
        let hubChallenge = ctx.TryGetQueryStringValue "hub.challenge"

        match hubChallenge with
        | None -> return! (text "no hub challenge" >> setStatusCode 400) next ctx
        | Some v ->
            let response = $"{{ \"hub.challenge\": \"%s{v}\" }}"
            return! (setBodyFromString response >> setContentType "application/json") next ctx
    }

let handlers: HttpFunc -> HttpContext -> HttpFuncResult =
    choose [ GET >=> route "/api/webhooks/strava" >=> getHandlerStrava ]
