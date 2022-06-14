module TokenAuthenticationExtensions

open System.Security.Claims
open Microsoft.AspNetCore.Authentication.JwtBearer
open Microsoft.AspNetCore.Builder
open Microsoft.Extensions.DependencyInjection
open Microsoft.IdentityModel.Tokens
open Saturn

type Saturn.Application.ApplicationBuilder with
    [<CustomOperation("use_token_authentication")>]
    member __.UseTokenAuthentication(state: ApplicationState) =
        let middleware (app: IApplicationBuilder) =
            app.UseAuthentication() |> ignore
            app


        let service (s: IServiceCollection) =
            s
                .AddAuthentication(fun options ->
                options.DefaultAuthenticateScheme <- JwtBearerDefaults.AuthenticationScheme
                options.DefaultChallengeScheme <- JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(fun options ->
                    options.Authority <- sprintf "https://%s/" "adventrunner.eu.auth0.com"
                    options.Audience <- "AdventRunner"

                    options.TokenValidationParameters <-
                        TokenValidationParameters(NameClaimType = ClaimTypes.NameIdentifier))
            |> ignore

            s

        { state with
              ServicesConfig = service :: state.ServicesConfig
              AppConfigs = middleware :: state.AppConfigs
              CookiesAlreadyAdded = true }
