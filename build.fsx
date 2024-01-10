#r "paket: groupref build //"
#load "./.fake/build.fsx/intellisense.fsx"
#r "netstandard"

open Fake.Core
open Fake.DotNet
open Fake.IO
open Farmer
open Farmer.Builders

Target.initEnvironment ()

let sharedPath = Path.getFullName "./src/Shared"
let serverPath = Path.getFullName "./src/Server"
let clientPath = Path.getFullName "./src/Client"
let deployDir = Path.getFullName "./deploy"
let sharedTestsPath = Path.getFullName "./tests/Shared"
let serverTestsPath = Path.getFullName "./tests/Server"

let yarn args workingDir =
    let yarnPath =
        match ProcessUtils.tryFindFileOnPath "yarn" with
        | Some path -> path
        | None ->
            "yarn was not found in path. Please install it and make sure it's available from your path. "
            |> failwith

    let arguments = args |> String.split ' ' |> Arguments.OfArgs

    Command.RawCommand (yarnPath, arguments)
    |> CreateProcess.fromCommand
    |> CreateProcess.withWorkingDirectory workingDir
    |> CreateProcess.ensureExitCode
    |> Proc.run
    |> ignore

let dotnet cmd workingDir =
    let result = DotNet.exec (DotNet.Options.withWorkingDirectory workingDir) cmd ""
    if result.ExitCode <> 0 then failwithf "'dotnet %s' failed in %s" cmd workingDir

Target.create "Clean" (fun _ -> Shell.cleanDir deployDir)

Target.create "InstallClient" (fun _ -> yarn "install" clientPath)

Target.create "Bundle" (fun _ ->
    dotnet (sprintf "publish -c Release -o \"%s\"" deployDir) serverPath
    yarn "build" clientPath
)

Target.create "Azure" (fun _ ->
    let web = webApp {
        name "AdventRunner"
        zip_deploy "deploy"
    }
    let deployment = arm {
        location Location.WestEurope
        add_resource web
    }

    deployment
    |> Deploy.execute "AdventRunner" Deploy.NoParameters
    |> ignore
)

Target.create "Run" (fun _ ->
    dotnet "build" sharedPath
    [ async { dotnet "watch run" serverPath }
      async { yarn "dev" clientPath } ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

Target.create "RunTests" (fun _ ->
    dotnet "build" sharedTestsPath
    [ async { dotnet "watch run" serverTestsPath } ]
    |> Async.Parallel
    |> Async.RunSynchronously
    |> ignore
)

open Fake.Core.TargetOperators

"Clean"
    ==> "InstallClient"
    ==> "Bundle"
    ==> "Azure"

"Clean"
    ==> "InstallClient"
    ==> "Run"

"Clean"
    ==> "InstallClient"
    ==> "RunTests"

Target.runOrDefaultWithArguments "Bundle"
