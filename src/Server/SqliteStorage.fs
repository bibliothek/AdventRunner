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
        calendarEntities
        |> Seq.map (fun calendarEntity ->
            let doorSql =
                "SELECT * FROM Doors WHERE OwnerName = @OwnerName AND Period = @Period"

            let doorEntities =
                connection.Query<DoorEntity>(
                    doorSql,
                    {| OwnerName = ownerName
                       Period = calendarEntity.Period |}
                )

            let doors = doorEntities |> Seq.map toDoor |> Seq.toList
            let calendar = toCalendar calendarEntity doors
            calendarEntity.Period, calendar)
        |> Map.ofSeq

    let conn = lazy(
            let c = createDbConnection ()
            c.Open()
            c
        )

    member private __.GetCalendarByPeriod (connection: DbConnection) ownerName period =
        let calendarSql = "SELECT * FROM Calendars WHERE OwnerName = @OwnerName AND Period = @Period"

        let calendarEntities =
            connection.Query<CalendarEntity>(calendarSql, {| OwnerName = ownerName ; Period = period |}) |> Seq.toArray

        getDoorsForCalendar calendarEntities ownerName connection

    member private __.GetCalendars (connection: DbConnection) ownerName =
        let calendarSql = "SELECT * FROM Calendars WHERE OwnerName = @OwnerName"

        let calendarEntities =
            connection.Query<CalendarEntity>(calendarSql, {| OwnerName = ownerName |}) |> Seq.toArray

        getDoorsForCalendar calendarEntities ownerName connection

    member __.GetUserData owner =
        let userSql = "SELECT * FROM Users WHERE Name = @Name"

        let userEntity =
            conn.Value.QueryFirst<UserEntity>(userSql, {| Name = getId owner |})

        let calendars = __.GetCalendars conn.Value (getId owner)
        toUser userEntity calendars

    member __.GetUserDataBySharedLink linkId =
        let calendarSql = "SELECT * FROM Calendars WHERE SharedLinkId = @SharedLinkId LIMIT 1"

        let calendarEntity = conn.Value.Query<CalendarEntity>(calendarSql, {| SharedLinkId = linkId |}) |> Seq.toList
        match calendarEntity with
        | [] -> None
        | head :: _ ->
            let userSql = "SELECT * FROM Users WHERE Name = @Name"
            let userEntity =
                conn.Value.QueryFirst<UserEntity>(userSql, {| Name = head.OwnerName |})
            let calendars = __.GetCalendarByPeriod conn.Value head.OwnerName head.Period
            Some(toUser userEntity calendars)

    member __.UserExists owner =
        let sql = "SELECT 1 FROM Users WHERE Name = @Name"
        conn.Value.ExecuteScalar<bool>(sql, {| Name = getId owner |})

    member private __.UpdateCalendars (connection: DbConnection) ownerName calendars =
        for KeyValue(period, calendar) in calendars do
            let calendarEntity = fromCalendar ownerName period calendar

            let calendarSql =
                "INSERT OR REPLACE INTO Calendars (OwnerName, Period, DistanceFactor, SharedLinkId, VerifiedDistance) VALUES (@OwnerName, @Period, @DistanceFactor, @SharedLinkId, @VerifiedDistance)"

            connection.Execute(calendarSql, calendarEntity) |> ignore

            for door in calendar.doors do
                let doorEntity = fromDoor ownerName period door

                let doorSql =
                    "INSERT OR REPLACE INTO Doors (OwnerName, Period, Day, Distance, State) VALUES (@OwnerName, @Period, @Day, @Distance, @State)"

                connection.Execute(doorSql, doorEntity) |> ignore

    member __.UpdateUserData(updatedUserData: UserData) =
        let userEntity = fromUser updatedUserData

        let userSql =
            "INSERT OR REPLACE INTO Users (Name, DisplayName, DisplayType, LatestPeriod) VALUES (@Name, @DisplayName, @DisplayType, @LatestPeriod)"

        conn.Value.Execute(userSql, userEntity) |> ignore
        __.UpdateCalendars conn.Value (getId updatedUserData.owner) updatedUserData.calendars
        updatedUserData

    member __.AddNewUser userData = __.UpdateUserData userData
