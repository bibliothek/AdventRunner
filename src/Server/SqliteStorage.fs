module Server.SqliteStorage

open System
open System.Data.Common
open System.Data.SQLite
open Dapper
open Shared

// #############
// DB Entities
// #############
[<AllowNullLiteral>]
type UserEntity() =
    member val Name = "" with get, set
    member val DisplayName = "" with get, set
    member val DisplayType = "" with get, set
    member val LatestPeriod = 0 with get, set

[<AllowNullLiteral>]
type CalendarEntity() =
    member val OwnerName = "" with get, set
    member val Period = 0 with get, set
    member val DistanceFactor = 0.0 with get, set
    member val SharedLinkId = "" with get, set
    member val VerifiedDistance = Nullable() with get, set

[<AllowNullLiteral>]
type DoorEntity() =
    member val OwnerName = "" with get, set
    member val Period = 0 with get, set
    member val Day = 0 with get, set
    member val Distance = 0.0 with get, set
    member val State = "" with get, set

// #############
// Mappers
// #############
let fromUser (user: UserData) : UserEntity =
    let entity = UserEntity()
    entity.Name <- user.owner.name
    entity.DisplayName <- user.displayName |> Option.toObj
    entity.DisplayType <- user.displayType.ToString()
    entity.LatestPeriod <- user.latestPeriod
    entity

let fromCalendar (ownerName: string) (period: int) (calendar: Calendar) : CalendarEntity =
    let entity = CalendarEntity()
    entity.OwnerName <- ownerName
    entity.Period <- period
    entity.DistanceFactor <- calendar.settings.distanceFactor
    entity.SharedLinkId <- calendar.settings.sharedLinkId |> Option.toObj
    entity.VerifiedDistance <- calendar.verifiedDistance |> Option.toNullable
    entity

let fromDoor (ownerName: string) (period: int) (door: CalendarDoor) : DoorEntity =
    let entity = DoorEntity()
    entity.OwnerName <- ownerName
    entity.Period <- period
    entity.Day <- door.day
    entity.Distance <- door.distance
    entity.State <- door.state.ToString()
    entity

let private displayTypeFromString (s: string) =
    match s with
    | "Monthly" -> Monthly
    | _ -> Door

let private doorStateFromString (s: string) =
    match s with
    | "Open" -> Open
    | "Done" -> Done
    | "Failed" -> Failed
    | _ -> Closed

let toUser (userEntity: UserEntity) (calendars: Map<int, Calendar>) : UserData =
    { owner = { name = userEntity.Name }
      displayName = userEntity.DisplayName |> Option.ofObj
      displayType = userEntity.DisplayType |> displayTypeFromString
      latestPeriod = userEntity.LatestPeriod
      calendars = calendars }

let toCalendar (calendarEntity: CalendarEntity) (doors: CalendarDoor list) : Calendar =
    { settings =
        { distanceFactor = calendarEntity.DistanceFactor
          sharedLinkId = calendarEntity.SharedLinkId |> Option.ofObj }
      doors = doors
      verifiedDistance = calendarEntity.VerifiedDistance |> Option.ofNullable }

let toDoor (doorEntity: DoorEntity) : CalendarDoor =
    { day = doorEntity.Day
      distance = doorEntity.Distance
      state = doorEntity.State |> doorStateFromString }

// #############
// DB
// #############
let private dbPath =
    Environment.GetEnvironmentVariable("AR_DbPath")
    |> Option.ofObj
    |> Option.defaultValue "advent_runner.db"

let private createDbConnection () : SQLiteConnection =
    new SQLiteConnection($"Data Source={dbPath}")

let private createTables (connection: SQLiteConnection) =
    let createUserTableSql =
        "CREATE TABLE IF NOT EXISTS Users (
            Name TEXT PRIMARY KEY,
            DisplayName TEXT,
            DisplayType TEXT,
            LatestPeriod INTEGER
         );"

    let createCalendarTableSql =
        "CREATE TABLE IF NOT EXISTS Calendars (
            OwnerName TEXT,
            Period INTEGER,
            DistanceFactor REAL,
            SharedLinkId TEXT,
            VerifiedDistance REAL,
            PRIMARY KEY (OwnerName, Period),
            FOREIGN KEY (OwnerName) REFERENCES Users(Name)
         );"

    let createDoorTableSql =
        "CREATE TABLE IF NOT EXISTS Doors (
            OwnerName TEXT,
            Period INTEGER,
            Day INTEGER,
            Distance REAL,
            State TEXT,
            PRIMARY KEY (OwnerName, Period, Day),
            FOREIGN KEY (OwnerName, Period) REFERENCES Calendars(OwnerName, Period)
         );"

    connection.Execute(createUserTableSql) |> ignore
    connection.Execute(createCalendarTableSql) |> ignore
    connection.Execute(createDoorTableSql) |> ignore

let init () =
    if not (System.IO.File.Exists(dbPath)) then
        SQLiteConnection.CreateFile(dbPath)
        use connection = createDbConnection ()
        connection.Open()
        createTables connection
        connection.Close()

type UserDataStorage() =
    let getId owner = owner.name

    let getDoorsForCalendar (calendarEntities: CalendarEntity array) ownerName (connection: DbConnection) =
        let periods = calendarEntities |> Seq.map (fun c -> c.Period) |> Seq.toArray
        if periods.Length = 0 then
            Map.empty
        else
            let doorSql = "SELECT * FROM Doors WHERE OwnerName = @OwnerName AND Period IN @Periods"
            let allDoorEntities =
                connection.Query<DoorEntity>(
                    doorSql,
                    {| OwnerName = ownerName; Periods = periods |}
                )
                |> Seq.groupBy (fun d -> d.Period)
                |> Map.ofSeq

            calendarEntities
            |> Seq.map (fun calendarEntity ->
                let doorEntities =
                    allDoorEntities
                    |> Map.tryFind calendarEntity.Period
                    |> Option.defaultValue Seq.empty

                let doors = doorEntities |> Seq.map toDoor |> Seq.toList
                let calendar = toCalendar calendarEntity doors
                calendarEntity.Period, calendar)
            |> Map.ofSeq

    let conn = lazy(
            let c = createDbConnection ()
            c.Open()
            c
        )

    member __.GetUserData owner =
        let sql =
            "SELECT u.*, c.* FROM Users u LEFT JOIN Calendars c ON u.Name = c.OwnerName WHERE u.Name = @Name"

        let data =
            conn.Value.Query<UserEntity, CalendarEntity, UserEntity * CalendarEntity>(
                sql,
                (fun user calendar -> user, calendar),
                {| Name = getId owner |},
                splitOn = "OwnerName"
            )
            |> Seq.toList

        match data with
        | [] -> None
        | (userEntity, _) :: _ ->
            let calendarEntities =
                data
                |> List.choose (fun (_, calendar) ->
                    if obj.ReferenceEquals(calendar, null) then None else Some calendar)
                |> List.toArray

            let calendars =
                getDoorsForCalendar calendarEntities (getId owner) conn.Value

            Some (toUser userEntity calendars)

    member __.GetUserDataBySharedLink linkId =
        let sql =
            "SELECT c.*, u.* FROM Calendars c JOIN Users u ON u.Name = c.OwnerName WHERE c.SharedLinkId = @SharedLinkId LIMIT 1"

        let data =
            conn.Value.Query<CalendarEntity, UserEntity, CalendarEntity * UserEntity>(
                sql,
                (fun calendar user -> calendar, user),
                {| SharedLinkId = linkId |},
                splitOn = "Name"
            )
            |> Seq.toList

        match data with
        | [] -> None
        | (calendar, user) :: _ ->
            let calendars =
                getDoorsForCalendar [| calendar |] user.Name conn.Value

            Some(toUser user calendars)

    member private __.UpdateCalendars (connection: DbConnection) ownerName calendars =
        if not (calendars |> Map.isEmpty) then
            let calendarEntities =
                calendars
                |> Map.toList
                |> List.map (fun (period, calendar) -> fromCalendar ownerName period calendar)

            let calendarSql =
                "INSERT OR REPLACE INTO Calendars (OwnerName, Period, DistanceFactor, SharedLinkId, VerifiedDistance) VALUES (@OwnerName, @Period, @DistanceFactor, @SharedLinkId, @VerifiedDistance)"

            connection.Execute(calendarSql, calendarEntities) |> ignore

            let doorEntities =
                calendars
                |> Map.toList
                |> List.collect (fun (period, calendar) ->
                    calendar.doors |> List.map (fun door -> fromDoor ownerName period door))

            if not (doorEntities |> List.isEmpty) then
                let doorSql =
                    "INSERT OR REPLACE INTO Doors (OwnerName, Period, Day, Distance, State) VALUES (@OwnerName, @Period, @Day, @Distance, @State)"

                connection.Execute(doorSql, doorEntities) |> ignore

    member __.UpdateUserData(updatedUserData: UserData) =
        let userEntity = fromUser updatedUserData

        let userSql =
            "INSERT OR REPLACE INTO Users (Name, DisplayName, DisplayType, LatestPeriod) VALUES (@Name, @DisplayName, @DisplayType, @LatestPeriod)"

        conn.Value.Execute(userSql, userEntity) |> ignore
        __.UpdateCalendars conn.Value (getId updatedUserData.owner) updatedUserData.calendars
        updatedUserData

    member __.AddNewUser userData = __.UpdateUserData userData
