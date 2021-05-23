module Storage

open System.IO
open System.Text
open Azure.Storage.Blobs
open Microsoft.Extensions.DependencyInjection
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



