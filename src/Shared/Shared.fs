namespace Shared

open System

type DoorState = Closed | Open | Done | Failed

type CalendarDoor =
    { day: int
      distance: double
      state: DoorState }

type Owner = { name: string }

type Settings = { distanceFactor: double }

type Calendar =
    { version: string
      settings: Settings
      doors: CalendarDoor list
      owner: Owner }

module Settings =
    let init factor = { distanceFactor = factor }

    let initDefault = init 1.0

module Calendar =
    let private initDoors (settings: Settings) =
        let days = seq { 1 .. 24 } |> Seq.toList
        let random = Random()
        let distances = days
                        |> List.map double
                        |> List.sortBy (fun _ -> random.Next())
        let doors =
            days
            |> List.map (fun d ->
                { day = d
                  distance = distances.Item(d - 1) * settings.distanceFactor
                  state = Closed })

        doors

    let init owner settings: Calendar =
        { version = "1.1"
          doors = initDoors settings
          owner = owner
          settings = settings }

module Route =
    let builder typeName methodName = sprintf "/api/%s/%s" typeName methodName

type IAdventRunApi =
    { createCalendar: Owner -> Async<Calendar>
      getCalendar: Owner -> Async<Calendar>
      updateCalendar: Calendar -> Async<Calendar> }
