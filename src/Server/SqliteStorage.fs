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

[<AllowNullLiteral>]
type SharedLinkEntity() =
    member val Id = "" with get, set
    member val OwnerName = "" with get, set
    member val Period = 0 with get, set

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

let fromSharedLink (sharedLink: SharedLink) : SharedLinkEntity =
    let entity = SharedLinkEntity()
    entity.Id <- sharedLink.id
    entity.OwnerName <- sharedLink.owner.name
    entity.Period <- sharedLink.period
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

let toSharedLink (sharedLinkEntity: SharedLinkEntity) : SharedLink =
    { id = sharedLinkEntity.Id
      owner = { name = sharedLinkEntity.OwnerName }
      period = sharedLinkEntity.Period }

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

    let createSharedLinkTableSql =
        "CREATE TABLE IF NOT EXISTS SharedLinks (
            Id TEXT PRIMARY KEY,
            OwnerName TEXT,
            Period INTEGER,
            FOREIGN KEY (OwnerName) REFERENCES Users(Name)
         );"

    connection.Execute(createUserTableSql) |> ignore
    connection.Execute(createCalendarTableSql) |> ignore
    connection.Execute(createDoorTableSql) |> ignore
    connection.Execute(createSharedLinkTableSql) |> ignore

let init () =
    if not (System.IO.File.Exists(dbPath)) then
        SQLiteConnection.CreateFile(dbPath)

    use connection = createDbConnection ()
    connection.Open()
    createTables connection

type UserDataStorage() =
    let getId owner = owner.name

    member private __.GetCalendars (connection: DbConnection) ownerName =
        let calendarSql = "SELECT * FROM Calendars WHERE OwnerName = @OwnerName"

        let calendarEntities =
            connection.Query<CalendarEntity>(calendarSql, {| OwnerName = ownerName |})

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

    member __.GetUserData owner =
        use connection = createDbConnection ()
        connection.Open()
        let userSql = "SELECT * FROM Users WHERE Name = @Name"

        let userEntity =
            connection.QueryFirst<UserEntity>(userSql, {| Name = getId owner |})

        let calendars = __.GetCalendars connection (getId owner)
        toUser userEntity calendars

    member __.UserExists owner =
        use connection = createDbConnection ()
        connection.Open()
        let sql = "SELECT 1 FROM Users WHERE Name = @Name"
        connection.ExecuteScalar<bool>(sql, {| Name = getId owner |})

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
        use connection = createDbConnection ()
        connection.Open()
        let userEntity = fromUser updatedUserData

        let userSql =
            "INSERT OR REPLACE INTO Users (Name, DisplayName, DisplayType, LatestPeriod) VALUES (@Name, @DisplayName, @DisplayType, @LatestPeriod)"

        connection.Execute(userSql, userEntity) |> ignore
        __.UpdateCalendars connection (getId updatedUserData.owner) updatedUserData.calendars
        updatedUserData

    member __.AddNewUser userData = __.UpdateUserData userData

type SharedLinksStorage() =
    member __.UpsertSharedLink sharedLink =
        use connection = createDbConnection ()
        connection.Open()
        let sharedLinkEntity = fromSharedLink sharedLink

        let sql =
            "INSERT OR REPLACE INTO SharedLinks (Id, OwnerName, Period) VALUES (@Id, @OwnerName, @Period)"

        connection.Execute(sql, sharedLinkEntity) |> ignore
        sharedLink

    member __.LinkExists sharedLinkId =
        use connection = createDbConnection ()
        connection.Open()
        let sql = "SELECT 1 FROM SharedLinks WHERE Id = @Id"
        connection.ExecuteScalar<bool>(sql, {| Id = sharedLinkId |})

    member __.GetSharedLink sharedLinkId =
        use connection = createDbConnection ()
        connection.Open()
        let sql = "SELECT * FROM SharedLinks WHERE Id = @Id"

        let sharedLinkEntityO =
            connection.QueryFirstOrDefault<SharedLinkEntity>(sql, {| Id = sharedLinkId |})

        if isNull sharedLinkEntityO then
            None
        else
            sharedLinkEntityO |> toSharedLink |> Some

    member __.DeleteSharedLink sharedLinkId =
        use connection = createDbConnection ()
        connection.Open()
        let sql = "DELETE FROM SharedLinks WHERE Id = @Id"
        connection.Execute(sql, {| Id = sharedLinkId |}) |> ignore