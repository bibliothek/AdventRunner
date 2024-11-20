module Server.Runner

open System.Reflection
open Microsoft.Extensions.Configuration
open Microsoft.Extensions.DependencyInjection
open Newtonsoft.Json
open Saturn
open Giraffe
open Server.Auth.TokenAuthenticationExtensions
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Server.Storage
open Server.MsgProcessor
open FluentMigrator.Runner

let serviceConfig (serviceCollection: IServiceCollection) =
    let jsonSerializer: Json.ISerializer =
        NewtonsoftJson.Serializer(JsonSerializerSettings())

    serviceCollection
        .AddSingleton<UserDataStorage>(fun provider -> UserDataStorage())
        .AddSingleton<SharedLinksStorage>(fun provider -> SharedLinksStorage())
        .AddSingleton<SyncQueue>(fun provider -> provider.GetRequiredService<UserDataStorage>() |> SyncQueue)
        .AddSingleton<Json.ISerializer>(fun provider -> jsonSerializer)
        .AddFluentMigratorCore()
        .ConfigureRunner(fun builder ->
            builder
                .AddSQLite()
                .ScanIn(Assembly.GetExecutingAssembly())
                .WithGlobalConnectionString(ConfigAccessor.getDbConnectionStringFromServiceProvider)
            |> ignore)

let webApp =
    choose
        [ WebhookEndpoints.handlers
          SharedLinkEndpoints.handlers
          CalendarEndpoints.handlers ]

let appBuilder =
    application {
        url "http://0.0.0.0:8085"
        use_token_authentication
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
        service_config serviceConfig
    }

let app = appBuilder.Build()
let sp = app.Services.CreateScope().ServiceProvider

Database.init sp

app.Run()
