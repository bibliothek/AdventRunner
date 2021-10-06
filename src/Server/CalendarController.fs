module CalendarController

open System.Security.Claims
open Microsoft.AspNetCore.Http
open Giraffe
open Shared
open Storage
open FSharp.Control.Tasks

let migrate (storage: Storage) cal =
    if cal.version <> "1.1" then
        let newCal = Calendar.init cal.owner Settings.initDefault
        Some (storage.AddNewCalendar newCal)
    else
        None

let getHandler next (ctx: HttpContext) =
    let ownerName = ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value
    let (owner: Owner) = {name = ownerName}
    let storage = ctx.GetService<Storage>()
    let cal =
        match (storage.CalendarExists owner) with
        | true ->
            let cal = storage.GetCalendar owner
            migrate storage cal |> Option.defaultValue cal
        | false ->
            storage.AddNewCalendar (Calendar.init owner Settings.initDefault)
    json cal next ctx

let putHandler next (ctx: HttpContext) =
    task {
        let ownerName = ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value
        let! cal = ctx.BindJsonAsync<Calendar>()
        let (owner: Owner) = {name = ownerName}
        let calWithOwner = {cal with owner = owner}
        let storage = ctx.GetService<Storage>()
        return! json (storage.UpdateCalendar calWithOwner) next ctx
    }

let postHandler next (ctx: HttpContext) =
    let ownerName = ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value
    let (owner: Owner) = {name = ownerName}
    let storage = ctx.GetService<Storage>()
    let cal = Calendar.init owner Settings.initDefault
    json (storage.AddNewCalendar cal) next ctx

let handlers: HttpFunc -> HttpContext -> HttpFuncResult =
    choose [
        GET
        >=> route "/api/calendars"
        >=> getHandler
        PUT
        >=> route "/api/calendars"
        >=> putHandler
        POST
        >=> route "/api/calendars"
        >=> postHandler
    ]