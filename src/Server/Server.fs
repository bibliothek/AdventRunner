module Server

open System.IO
open System.Security.Claims
open System.Text
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.DependencyInjection
open Saturn
open Giraffe
open Azure.Storage.Blobs
open Newtonsoft.Json
open Shared
open TokenAuthenticationExtensions

open Storage

let notLoggedIn =
    setStatusCode 403 >=> text "Forbidden"

let mustBeLoggedIn = requiresAuthentication notLoggedIn

let serviceConfig (serviceCollection: IServiceCollection) =
    serviceCollection.AddSingleton<Storage>(fun provider -> Storage())

let webApp =
    mustBeLoggedIn >=> CalendarController.handlers

let app =
    application {
        url "http://0.0.0.0:8085"
        use_token_authentication
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
        service_config serviceConfig
    }

run app
