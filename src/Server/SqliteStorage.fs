
module Server.SqliteStorage

open System.Data.Common
open System.Data.SQLite
open Dapper
open Shared

let private dbPath = "advent_runner.db"

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

type UserDataStorage () =
    let getId owner = owner.name

    member private __.GetCalendars (connection: DbConnection) ownerName =
        let calendarSql = "SELECT Period, DistanceFactor, SharedLinkId, VerifiedDistance FROM Calendars WHERE OwnerName = @OwnerName"
        let calendars = connection.Query<obj>(calendarSql, {| OwnerName = ownerName |})
        calendars
        |> Seq.map (fun cal ->
            let p = cal :?> System.Collections.Generic.IDictionary<string, obj>
            let period = p.["Period"] :?> int64 |> int
            let doorSql = "SELECT Day, Distance, State FROM Doors WHERE OwnerName = @OwnerName AND Period = @Period"
            let doors = connection.Query<CalendarDoor>(doorSql, {| OwnerName = ownerName; Period = period |}) |> Seq.toList
            let settings = { distanceFactor = p.["DistanceFactor"] :?> double; sharedLinkId = p.["SharedLinkId"] :?> string |> Some }
            let verifiedDistance = match p.["VerifiedDistance"] with | null -> None | x -> Some (x :?> float)
            let calendar = { settings = settings; doors = doors; verifiedDistance = verifiedDistance }
            period, calendar)
        |> Map.ofSeq

    member __.GetUserData owner =
        use connection = createDbConnection ()
        connection.Open()
        let userSql = "SELECT Name, DisplayName, DisplayType, LatestPeriod FROM Users WHERE Name = @Name"
        let user = connection.QueryFirst<UserData>(userSql, {| Name = getId owner |})
        let calendars = __.GetCalendars connection (getId owner)
        { user with calendars = calendars }

    member __.UserExists owner =
        use connection = createDbConnection ()
        connection.Open()
        let sql = "SELECT 1 FROM Users WHERE Name = @Name"
        connection.ExecuteScalar<bool>(sql, {| Name = getId owner |})

    member private __.UpdateCalendars (connection: DbConnection) ownerName calendars =
        for KeyValue(period, calendar) in calendars do
            let calendarSql = "INSERT OR REPLACE INTO Calendars (OwnerName, Period, DistanceFactor, SharedLinkId, VerifiedDistance) VALUES (@OwnerName, @Period, @DistanceFactor, @SharedLinkId, @VerifiedDistance)"
            connection.Execute(calendarSql, {| OwnerName = ownerName; Period = period; DistanceFactor = calendar.settings.distanceFactor; SharedLinkId = calendar.settings.sharedLinkId; VerifiedDistance = calendar.verifiedDistance |}) |> ignore
            for door in calendar.doors do
                let doorSql = "INSERT OR REPLACE INTO Doors (OwnerName, Period, Day, Distance, State) VALUES (@OwnerName, @Period, @Day, @Distance, @State)"
                connection.Execute(doorSql, {| OwnerName = ownerName; Period = period; Day = door.day; Distance = door.distance; State = door.state.ToString() |}) |> ignore

    member __.UpdateUserData (updatedUserData: UserData) =
        use connection = createDbConnection ()
        connection.Open()
        let userSql = "INSERT OR REPLACE INTO Users (Name, DisplayName, DisplayType, LatestPeriod) VALUES (@Name, @DisplayName, @DisplayType, @LatestPeriod)"
        connection.Execute(userSql, {| Name = getId updatedUserData.owner; DisplayName = updatedUserData.displayName |> Option.toObj ; DisplayType = updatedUserData.displayType.ToString(); LatestPeriod = updatedUserData.latestPeriod |}) |> ignore
        __.UpdateCalendars connection (getId updatedUserData.owner) updatedUserData.calendars
        updatedUserData

    member __.AddNewUser userData =
        __.UpdateUserData userData

type SharedLinksStorage () =
    let getId sharedLink = sharedLink.id

    member __.UpsertSharedLink sharedLink =
        use connection = createDbConnection ()
        connection.Open()
        let sql = "INSERT OR REPLACE INTO SharedLinks (Id, OwnerName, Period) VALUES (@Id, @OwnerName, @Period)"
        connection.Execute(sql, {| Id = sharedLink.id; OwnerName = sharedLink.owner.name; Period = sharedLink.period |}) |> ignore
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
        connection.QueryFirstOrDefault<SharedLink>(sql, {| Id = sharedLinkId |})

    member __.DeleteSharedLink sharedLinkId =
        use connection = createDbConnection ()
        connection.Open()
        let sql = "DELETE FROM SharedLinks WHERE Id = @Id"
        connection.Execute(sql, {| Id = sharedLinkId |}) |> ignore
