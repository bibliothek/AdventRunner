module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn

open Shared

type Storage () =
    let calendars = ResizeArray<Calendar>()

    member __.GetCalendar owner =
        calendars.Find (fun c -> c.owner = owner)

    member __.UpdateCalendar updatedCalendar =
        let i = calendars.FindIndex (fun c -> c.owner = updatedCalendar.owner)
        calendars.RemoveAt(i)
        calendars.Add updatedCalendar
        updatedCalendar

    member __.AddNewCalendar calendar =
        calendars.Add calendar
        calendar

let storage = Storage()

let cal = Calendar.init {name = "matha"}

storage.AddNewCalendar cal |> ignore

let adventRunApi : IAdventRunApi =
    { createCalendar = fun owner -> async {
        let cal = Calendar.init owner
        return storage.AddNewCalendar cal
        }
      getCalendar = fun owner -> async {
          return storage.GetCalendar owner
      }
      updateCalendar = fun calendar -> async {
          return storage.UpdateCalendar calendar
      }
    }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue adventRunApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
