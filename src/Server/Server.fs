module Server

open System.IO
open System.Text
open Fable.Remoting.Server
open Fable.Remoting.Giraffe
open Saturn
open Azure.Storage.Blobs
open Newtonsoft.Json
open Shared

type Storage () =
    let getContainerClient =
        let connectionString = System.Environment.GetEnvironmentVariable "AR_StorageAccount_ConnectionString"
        let blobServiceClient = BlobServiceClient connectionString
        let containerName = "calendars"
        blobServiceClient.GetBlobContainerClient containerName

    let getBlobClient owner =
        let containerClient = getContainerClient
        containerClient.GetBlobClient (sprintf "%s.txt" owner.name)

    let uploadCalendar calendar =
        let serializedCalendar = JsonConvert.SerializeObject calendar
        let blobClient = getBlobClient calendar.owner
        use stream = new MemoryStream (Encoding.UTF8.GetBytes serializedCalendar)
        blobClient.Upload(stream, true) |> ignore

    member __.GetCalendar owner =
        let blobClient = getBlobClient owner
        use stream = new MemoryStream()
        blobClient.DownloadTo stream |> ignore
        stream.Position <- 0L
        use reader = new StreamReader(stream, Encoding.UTF8)
        let serializedCalendar = reader.ReadToEnd()
        let cal = JsonConvert.DeserializeObject<Calendar> serializedCalendar
        cal

    member __.CalendarExists owner =
        let blobClient = getBlobClient owner
        blobClient.Exists().Value

    member __.UpdateCalendar updatedCalendar =
        uploadCalendar updatedCalendar
        updatedCalendar

    member __.AddNewCalendar calendar =
        uploadCalendar calendar
        calendar

let storage = Storage()

let adventRunApi : IAdventRunApi =
    { createCalendar = fun owner -> async {
        let cal = Calendar.init owner
        return storage.AddNewCalendar cal
        }
      getCalendar = fun owner -> async {
          match (storage.CalendarExists owner) with
          | true -> return storage.GetCalendar owner
          | false ->
              let newCalendar = Calendar.init owner
              return storage.AddNewCalendar newCalendar
      }
      updateCalendar = fun calendar -> async {
          return storage.UpdateCalendar calendar
      }
    }

let webApp =
    Remoting.createApi()
    |> Remoting.withRouteBuilder Route.builder
    |> Remoting.fromValue adventRunApi
    |> Remoting.buildHttpHandler

let app =
    application {
        url "http://0.0.0.0:8085"
        use_router webApp
        memory_cache
        use_static "public"
        use_gzip
    }

run app
