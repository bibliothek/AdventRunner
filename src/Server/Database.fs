module Server.Database

open System
open System.IO
open Dapper
open FluentMigrator.Runner
open Microsoft.AspNetCore.Http
open Microsoft.Data.Sqlite
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.DependencyInjection

type UserData = { id: string; data: string }

let private ensureDb (sp: IServiceProvider) =
    let config = sp.GetService<IConfiguration>()
    let path = config["AR_Db_Path"]

    if not (File.Exists path) then
        File.Create(path).Close()

let private migrate (sp: IServiceProvider) =
    let runner = sp.GetService<IMigrationRunner> ()
    runner.MigrateUp()

let init (sp: IServiceProvider) =
    ensureDb sp
    migrate sp

let getUser id (ctx: HttpContext) =
    let connectionString = ConfigAccessor.getDbConnectionString ctx
    let conn = new SqliteConnection(connectionString)

    let userData =
        conn.QuerySingleOrDefault<UserData>("select id, data from UserData where id =@id", id)

    conn.Dispose()
    userData
