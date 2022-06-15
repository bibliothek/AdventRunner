module Storage

open System.IO
open System.Text
open Azure.Storage.Blobs
open Newtonsoft.Json
open Shared

let private getContainerClient containerName =
        let connectionString = System.Environment.GetEnvironmentVariable "AR_StorageAccount_ConnectionString"
        let blobServiceClient = BlobServiceClient connectionString
        blobServiceClient.GetBlobContainerClient containerName

let private getBlobClient containerName id =
        let containerClient = getContainerClient containerName
        containerClient.GetBlobClient (sprintf "%s.json" id)

let private uploadData containerName id data =
        let serializedCalendar = JsonConvert.SerializeObject data
        let blobClient = getBlobClient containerName id
        use stream = new MemoryStream (Encoding.UTF8.GetBytes serializedCalendar)
        blobClient.Upload(stream, true) |> ignore

let private elementExists containerName id =
        let blobClient = getBlobClient containerName id
        blobClient.Exists().Value

let private getData<'T> containerName id =
        let blobClient = getBlobClient containerName id
        use stream = new MemoryStream()
        blobClient.DownloadTo stream |> ignore
        stream.Position <- 0L
        use reader = new StreamReader(stream, Encoding.UTF8)
        let serializedCalendar = reader.ReadToEnd()
        let data = JsonConvert.DeserializeObject<'T> serializedCalendar
        data

type UserDataStorage () =
    let containerName = "users"
    let getId owner = owner.name

    member __.GetUserData owner =
        getData<UserData> containerName (getId owner)

    member __.UserExists owner =
        let blobClient = getBlobClient containerName (getId owner)
        blobClient.Exists().Value

    member __.UpdateUserData (updatedUserData: UserData) =
        uploadData containerName (getId updatedUserData.owner) updatedUserData
        updatedUserData

    member __.AddNewUser userData =
        uploadData containerName (getId userData.owner) userData
        userData

type SharedLinksStorage () =
    let containerName = "shared-links"
    let getId sharedLink = sharedLink.id

    member __.UpsertSharedLink sharedLink =
        uploadData containerName (getId sharedLink) sharedLink
        sharedLink

    member __.GetSharedLink sharedLinkId =
        getData<SharedLink> containerName sharedLinkId
