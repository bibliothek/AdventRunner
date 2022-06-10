﻿module CalendarController

open System.Security.Claims
open Microsoft.AspNetCore.Http
open Giraffe
open Shared
open Storage
open FSharp.Control.Tasks

let migrate (storage: Storage) (userData: UserData) =
    if userData.version <> "2.0" then
        let userData = Calendar.initUserData userData.owner Settings.initDefault
        Some (storage.AddNewUser userData)
    elif userData.calendars.ContainsKey(Calendar.currentPeriod) <> true then
        let _,previousCalendar = userData.calendars |> Map.toList |> List.sortByDescending fst |> List.head
        let userDataWithNewPeriod = {userData with calendars = userData.calendars.Add (Calendar.currentPeriod, Calendar.initCalendar previousCalendar.settings)}
        Some (storage.UpdateUserData userDataWithNewPeriod)
    else
        None

let getHandler next (ctx: HttpContext) =
    let ownerName = ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value
    let (owner: Owner) = {name = ownerName}
    let storage = ctx.GetService<Storage>()
    let cal =
        match (storage.UserExists owner) with
        | true ->
            let cal = storage.GetUserData owner
            migrate storage cal |> Option.defaultValue cal
        | false ->
            storage.AddNewUser (Calendar.initUserData owner Settings.initDefault)
    json cal next ctx

let putHandler next (ctx: HttpContext) =
    task {
        let ownerName = ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value
        let! userData = ctx.BindJsonAsync<UserData>()
        let (owner: Owner) = {name = ownerName}
        let userDataWithOwner = {userData with owner = owner}
        let storage = ctx.GetService<Storage>()
        return! json (storage.UpdateUserData userDataWithOwner) next ctx
    }

let postHandler next (ctx: HttpContext) =
    let ownerName = ctx.User.FindFirst(ClaimTypes.NameIdentifier).Value
    let (owner: Owner) = {name = ownerName}
    let storage = ctx.GetService<Storage>()
    let cal = Calendar.initUserData owner Settings.initDefault
    json (storage.AddNewUser cal) next ctx

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