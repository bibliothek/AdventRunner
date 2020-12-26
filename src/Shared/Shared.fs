namespace Shared

open System

type DoorState = Closed | Open | Done | Failed

type CalendarDoor =
    { day: int
      distance: int
      state: DoorState }

type Owner = { name: string }

type Calendar =
    { version: string
      doors: CalendarDoor list
      owner: Owner }

module Calendar =
    let private initDoors (): CalendarDoor list =
        let days = seq { 1 .. 24 } |> Seq.toList
        let random = Random()

        let distances =
            days |> List.sortBy (fun _ -> random.Next())

        let doors =
            days
            |> List.map (fun d ->
                { day = d
                  distance = distances.Item(d - 1)
                  state = Closed })

        doors

    let init owner: Calendar =
        { version = "1"; doors = initDoors(); owner = owner }

module Route =
    let builder typeName methodName = sprintf "/api/%s/%s" typeName methodName

type IAdventRunApi =
    { createCalendar: Owner -> Async<Calendar>
      getCalendar: Owner -> Async<Calendar>
      updateCalendar: Calendar -> Async<Calendar> }
