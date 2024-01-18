module Server.WebhookEndpoints

open System.Text
open Microsoft.AspNetCore.Http
open Giraffe

let getHandlerStrava next (ctx: HttpContext) : HttpFuncResult =
    task {
        let hubChallenge = ctx.TryGetQueryStringValue "hub.challenge"
        let response = $"{{ \"hub.challenge\": \"%s{hubChallenge.Value}\" }}"
        let bytes = Encoding.UTF8.GetBytes response
        ctx.SetContentType "application/json"
        let! c = (ctx.WriteBytesAsync bytes)
        return! next c.Value
    }

let handlers: HttpFunc -> HttpContext -> HttpFuncResult =
    choose [ GET >=> route "/api/webhooks/strava" >=> getHandlerStrava ]
