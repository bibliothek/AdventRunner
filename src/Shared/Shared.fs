namespace Shared

open System

type CalendarDoor =
    { day: int
      distance: int
      opened: bool
      finished: bool }

type Owner = {
    name: string
}

type Calendar =
    {
        doors: CalendarDoor list
        owner: Owner
    }

module Calendar =
    let initDoors : CalendarDoor list =
        let days = seq {1 .. 24} |> Seq.toList
        let random = Random()
        let distances = days |> List.sortBy (fun _ -> random.Next())
        let doors = days |> List.map (fun d -> { day = d; distance = distances.Item (d - 1); opened = false; finished = false })
        doors

    let init owner : Calendar =
        {
            doors = initDoors
            owner = owner
        }

type Todo =
    { Id : Guid
      Description : string }

module Todo =
    let isValid (description: string) =
        String.IsNullOrWhiteSpace description |> not

    let create (description: string) =
        { Id = Guid.NewGuid()
          Description = description }

module Route =
    let builder typeName methodName =
        sprintf "/api/%s/%s" typeName methodName

type ITodosApi =
    { getTodos : unit -> Async<Todo list>
      addTodo : Todo -> Async<Todo> }

type IAdventRunApi =
    {
        createCalendar : Owner -> Async<Calendar>
        getCalendar : Owner -> Async<Calendar>
        updateCalendar : Calendar -> Async<Calendar>
    }