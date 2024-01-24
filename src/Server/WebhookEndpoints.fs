module Server.WebhookEndpoints

open System
open Microsoft.AspNetCore.Http
open Giraffe
open Microsoft.Extensions.Logging
open Newtonsoft.Json
open Server.MsgProcessor
open Shared
open Server.StravaSync

let getHandlerStrava next (ctx: HttpContext) : HttpFuncResult =
    task {
        let hubChallenge = ctx.TryGetQueryStringValue "hub.challenge"

        match hubChallenge with
        | None -> return! (text "no hub challenge" >> setStatusCode 400) next ctx
        | Some v ->
            let response = $"{{ \"hub.challenge\": \"%s{v}\" }}"
            return! (setBodyFromString response >> setContentType "application/json") next ctx
    }

type StravaEventData =
    { object_type: string
      owner_id: int64
      event_time: int64 }

let postHandlerStrava next (ctx: HttpContext) : HttpFuncResult =
    task {
        let! requestData = ctx.ReadBodyFromRequestAsync()
        let eventData = JsonConvert.DeserializeObject<StravaEventData> requestData
        let logger = ctx.GetLogger "WebhookEndpoints:postHandlerStrava"

        if eventData.object_type <> "activity" then
            logger.LogInformation ("Ignoring received object of type: {ObjectType}", eventData.object_type)
            return! setStatusCode 200 next ctx
        else
            logger.LogInformation ("Received activity update for {OwnerId}", eventData.owner_id)
            let owner  = { name = $"oauth2|Strava|{eventData.owner_id}" }
            let period = (DateTimeOffset.FromUnixTimeSeconds eventData.event_time).Year
            let syncQueue = ctx.GetService<SyncQueue> ()
            syncQueue.Enqueue {owner = owner; period = Period period }
            return! setStatusCode 200 next ctx
    }

let handlers: HttpFunc -> HttpContext -> HttpFuncResult =
    choose
        [ GET >=> route "/api/webhooks/strava" >=> getHandlerStrava
          POST >=> route "/api/webhooks/strava" >=> postHandlerStrava ]
