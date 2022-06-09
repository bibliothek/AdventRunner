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
        let containerName = "users"
        blobServiceClient.GetBlobContainerClient containerName

    let getBlobClient owner =
        let containerClient = getContainerClient
        containerClient.GetBlobClient (sprintf "%s.json" owner.name)

    let uploadUserData userData =
        let serializedCalendar = JsonConvert.SerializeObject userData
        let blobClient = getBlobClient userData.owner
        use stream = new MemoryStream (Encoding.UTF8.GetBytes serializedCalendar)
        blobClient.Upload(stream, true) |> ignore

    member __.GetUserData owner =
        let blobClient = getBlobClient owner
        use stream = new MemoryStream()
        blobClient.DownloadTo stream |> ignore
        stream.Position <- 0L
        use reader = new StreamReader(stream, Encoding.UTF8)
        let serializedCalendar = reader.ReadToEnd()
        let userData = JsonConvert.DeserializeObject<UserData> serializedCalendar
        userData

    member __.UserExists owner =
        let blobClient = getBlobClient owner
        blobClient.Exists().Value

    member __.UpdateUserData updatedUserData =
        uploadUserData updatedUserData
        updatedUserData

    member __.AddNewUser userData =
        uploadUserData userData
        userData



