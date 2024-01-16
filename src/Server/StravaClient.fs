module Server.StravaClient

open Auth0Client
open Flurl.Http

let clientId = System.Environment.GetEnvironmentVariable "AR_Strava_ClientId"
let clientSecret = System.Environment.GetEnvironmentVariable "AR_Strava_ClientSecret"

let getAccessToken (refreshToken: string) =
    task {
        let! response =
            "https://www.strava.com/oauth/token"
                .PostJsonAsync(
                    {| grant_type = "refresh_token"
                       client_id = clientId
                       client_secret = clientSecret
                       refresh_token = refreshToken |}
                )
                .ReceiveJson<TokenResponse>()

        return response.access_token
    }

type Activity =
    { distance: float
      id: int64
      sport_type: string
      start_date: string
      start_date_local: string }

let getActivities (accessToken: string) (after: int64, before:int64) =
    task {
        let! response =
            "https://www.strava.com/api/v3/athlete/activities"
                .WithHeader("Authorization", $"Bearer {accessToken}")
                .AppendQueryParam("per_page", 100)
                .AppendQueryParam("before", before)
                .AppendQueryParam("after", after)
                .GetJsonAsync<Activity[]>()

        return response
    }
