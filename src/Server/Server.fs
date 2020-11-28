module Server

open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Microsoft.AspNetCore.Mvc.TagHelpers.Cache
open Saturn

open Shared

type Storage () =
    let calendars = ResizeArray<Calendar>()
    let todos = ResizeArray<_>()

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

    member __.GetTodos () =
        List.ofSeq todos

    member __.AddTodo (todo: Todo) =
        if Todo.isValid todo.Description then
            todos.Add todo
            Ok ()
        else Error "Invalid todo"

let storage = Storage()

storage.AddTodo(Todo.create "Create new SAFE project") |> ignore
storage.AddTodo(Todo.create "Write your app") |> ignore
storage.AddTodo(Todo.create "Ship it !!!") |> ignore

let cal = Calendar.init {name = "matha"}

storage.AddNewCalendar cal |> ignore

let todosApi =
    { getTodos = fun () -> async { return storage.GetTodos() }
      addTodo =
        fun todo -> async {
            match storage.AddTodo todo with
            | Ok () -> return todo
            | Error e -> return failwith e
        } }

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
