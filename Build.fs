open Fake.Core
open Fake.IO
open Farmer
open Farmer.Builders

open Helpers

initializeContext ()

let sharedPath = Path.getFullName "src/Shared"
let serverPath = Path.getFullName "src/Server"
let clientPath = Path.getFullName "src/Client"
let deployPath = Path.getFullName "deploy"
let sharedTestsPath = Path.getFullName "tests/Shared"
let serverTestsPath = Path.getFullName "tests/Server"
let clientTestsPath = Path.getFullName "tests/Client"

Target.create "Clean" (fun _ ->
    Shell.cleanDir deployPath
)

Target.create "InstallClient" (fun _ -> run yarn "install" clientPath)

Target.create "Bundle" (fun _ ->
    [
        "server", dotnet [ "publish"; "-c"; "Release"; "-o"; deployPath ] serverPath
        "client", yarn "install" clientPath
    ]
    |> runParallel)

Target.create "Run" (fun _ ->
    run dotnet [ "build" ] sharedPath

    [
        "server", dotnet [ "watch"; "run" ] serverPath
        "client", yarn "dev" clientPath
    ]
    |> runParallel)

Target.create "RunTests" (fun _ ->
    run dotnet [ "build" ] sharedTestsPath

    [
        "server", dotnet [ "watch"; "run" ] serverTestsPath
    ]
    |> runParallel)

open Fake.Core.TargetOperators

let dependencies = [
    "Clean" ==> "InstallClient" ==> "Run"

    "InstallClient" ==> "RunTests"
]

[<EntryPoint>]
let main args = runOrDefault args