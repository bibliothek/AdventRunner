module Server.Auth0Client

open Shared
open Flurl.Http

type TokenResponse =
    { access_token: string
      expires_in: int
      scope: string
      token_type: string }

type IdentityProfile =
    { access_token: string
      connection: string
      isSocial: bool
      provider: string
      refresh_token: string
      user_id: string }

type UserResponse =
    { created_at: string
      identities: IdentityProfile[]
      last_ip: string
      last_login: string
      logins_count: int
      name: string
      nickname: string
      picture: string
      updated_at: string
      user_id: string }

let auth0BaseUrl = "https://adventrunner.eu.auth0.com"
let auth0Audience = $"{auth0BaseUrl}/api/v2/"

let clientId = System.Environment.GetEnvironmentVariable "AR_Auth0_ClientId"

let clientSecret = System.Environment.GetEnvironmentVariable "AR_Auth0_ClientSecret"

let canVerifyDistance (owner:Owner) =
    owner.name.Contains("|Strava|")

let getManagementAccessTokenAsync =
    task {
        let! response =
            $"{auth0BaseUrl}/oauth/token"
                .PostJsonAsync(
                    {| grant_type = "client_credentials"
                       client_id = clientId
                       client_secret = clientSecret
                       audience = auth0Audience |}
                )
                .ReceiveJson<TokenResponse>()

        return response.access_token
    }

let getStravaRefreshToken (accessToken: string) (owner: Owner) =
    task {
        if owner |> canVerifyDistance then
            let! response =
                $"{auth0Audience}users/{owner.name}"
                    .WithHeader("Authorization", $"Bearer {accessToken}")
                    .GetJsonAsync<UserResponse>()

            return
                response.identities
                |> Array.tryFind (fun el -> el.connection = "Strava")
                |> Option.bind (fun x -> Some x.refresh_token)

        else
            return None
    }
