module Server.Storage

open System.IO
open Newtonsoft.Json
open Shared

let private getStoragePath () =
    System.Environment.GetEnvironmentVariable "AR_Storage_Path"
    |> Option.ofObj
    |> Option.defaultValue ".data"

let private getPath containerName (id: string) =
    let storagePath = getStoragePath ()
    let containerPath = Path.Combine(storagePath, containerName)

    if not (Directory.Exists containerPath) then
        Directory.CreateDirectory containerPath |> ignore

    let fileName = id.Replace('|', '-')
    Path.Combine(containerPath, $"%s{fileName}.json")

let private uploadData containerName id data =
    let path = getPath containerName id
    let serializedData = JsonConvert.SerializeObject data
    File.WriteAllText(path, serializedData)

let private dataExists containerName id =
    let path = getPath containerName id
    File.Exists path

let private delete containerName id =
    let path = getPath containerName id
    File.Delete path

let private getData<'T> containerName id =
    let path = getPath containerName id
    let serializedData = File.ReadAllText path
    JsonConvert.DeserializeObject<'T> serializedData

type UserDataStorage() =
    let containerName = "users"
    let getId owner = owner.name

    member __.GetUserData owner =
        getData<UserData> containerName (getId owner)

    member __.UserExists owner = dataExists containerName (getId owner)

    member __.UpdateUserData(updatedUserData: UserData) =
        uploadData containerName (getId updatedUserData.owner) updatedUserData
        updatedUserData

    member __.AddNewUser userData =
        uploadData containerName (getId userData.owner) userData
        userData

type SharedLinksStorage() =
    let containerName = "shared-links"
    let getId sharedLink = sharedLink.id

    member __.UpsertSharedLink sharedLink =
        uploadData containerName (getId sharedLink) sharedLink
        sharedLink

    member __.LinkExists sharedLinkId = dataExists containerName sharedLinkId

    member __.GetSharedLink sharedLinkId =
        getData<SharedLink> containerName sharedLinkId

    member __.DeleteSharedLink sharedLinkId = delete containerName sharedLinkId