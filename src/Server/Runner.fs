module Server.Runner

open Microsoft.Extensions.DependencyInjection
open Saturn
open Giraffe
open Server.Auth.TokenAuthenticationExtensions

open Storage

let serviceConfig (serviceCollection: IServiceCollection) =
    serviceCollection
        .AddSingleton<UserDataStorage>(fun provider -> UserDataStorage())
        .AddSingleton<SharedLinksStorage>(fun provider -> SharedLinksStorage())

let webApp =
    choose [ SharedLinkEndpoints.handlers
             CalendarEndpoints.handlers ]


let app =
    application {
        url "http://0.0.0.0:8085"
        use_token_authentication
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
        service_config serviceConfig
    }

run app
