namespace Shared

open System

type DoorState = Closed | Open | Done | Failed

type CalendarDoor =
    { day: int
      distance: double
      state: DoorState }

type Owner = { name: string }
type SharedLinkId = string

type Settings = { distanceFactor: double; sharedLinkId: SharedLinkId option }

type SharedLink = {
    id: SharedLinkId
    owner: Owner
    period: int
}

type Calendar =
    { settings: Settings
      doors: CalendarDoor list
    }

type UserData =
    {
       version: string
       owner: Owner
       displayName: string option
       calendars: Map<int, Calendar>
       latestPeriod: int
    }

module Settings =
    let init factor = { distanceFactor = factor; sharedLinkId = None}

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

    let currentPeriod () =
        DateTime.UtcNow.Year

    let initCalendar settings: Calendar =
        { doors = initDoors settings
          settings = settings }

    let initUserData owner settings: UserData =
        let period = currentPeriod()
        { owner = owner
          version = "2.0"
          calendars = Map [(period, (initCalendar settings))]
          latestPeriod = period
          displayName = None}
