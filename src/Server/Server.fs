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

let storage = Storage()

let migrate cal =
    if cal.version <> "1.1" then
        let newCal = Calendar.init cal.owner Settings.initDefault
        Some (storage.AddNewCalendar newCal)
    else
        None

let adventRunApi : IAdventRunApi =
    { createCalendar = fun owner -> async {
        let cal = Calendar.init owner Settings.initDefault
        return storage.AddNewCalendar cal
        }
      getCalendar = fun owner -> async {
          match (storage.CalendarExists owner) with
          | true ->
              let cal = storage.GetCalendar owner
              return migrate cal |> Option.defaultValue cal
          | false ->
              let newCalendar = Calendar.init owner Settings.initDefault
              return storage.AddNewCalendar newCalendar
      }
      updateCalendar = fun calendar -> async {
          return storage.UpdateCalendar calendar
      }
    }

let notLoggedIn =
    setStatusCode 403 >=> text "Forbidden"

let mustBeLoggedIn = requiresAuthentication notLoggedIn

//let serviceConfig (serviceCollection: IServiceCollection) =
//    serviceCollection.AddSingleton<Storage>(fun provider -> Storage())


let calendarApi =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue adventRunApi
    |> Remoting.buildHttpHandler

let webApp =
    choose [
//        route "/" >=>
            mustBeLoggedIn >=>
                calendarApi
    ]

let app =
    application {
        url "http://0.0.0.0:8085"
        use_token_authentication
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
