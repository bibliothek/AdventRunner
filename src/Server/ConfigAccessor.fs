module Server.ConfigAccessor

open System
open Microsoft.AspNetCore.Http
open Giraffe
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection

let private getDbConnectionStringFromConfig (config: IConfiguration) = sprintf "Data Source=%s;" config["AR_Db_Path"]

let getDbConnectionStringFromServiceProvider (sp: IServiceProvider) = sp.GetService<IConfiguration>() |> getDbConnectionStringFromConfig

let getDbConnectionString (ctx: HttpContext) = ctx.GetService<IConfiguration>() |> getDbConnectionStringFromConfig
