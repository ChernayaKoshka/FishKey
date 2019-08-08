#nowarn "FAKE0001"

// --------------------------------------------------------------------------------------
// FAKE build script
// --------------------------------------------------------------------------------------

#r "./packages/build/FAKE/tools/FakeLib.dll"

open Fake
open System

// --------------------------------------------------------------------------------------
// Build variables
// --------------------------------------------------------------------------------------

let buildDir  = "./build/"
let appReferences = !! "/**/*.fsproj"
let dotnetcliVersion = "2.0.2"
let mutable dotnetExePath = "dotnet"

// --------------------------------------------------------------------------------------
// Helpers
// --------------------------------------------------------------------------------------

let run' timeout cmd args dir =
    if execProcess (fun info ->
        info.FileName <- cmd
        if not (String.IsNullOrWhiteSpace dir) then
            info.WorkingDirectory <- dir
        info.Arguments <- args
    ) timeout |> not then
        failwithf "Error while running '%s' with args: %s" cmd args

let run = run' System.TimeSpan.MaxValue

let runDotnet workingDir args =
    let result =
        ExecProcess (fun info ->
            info.FileName <- dotnetExePath
            info.WorkingDirectory <- workingDir
            info.Arguments <- args) TimeSpan.MaxValue
    if result <> 0 then failwithf "dotnet %s failed" args

// --------------------------------------------------------------------------------------
// Targets
// --------------------------------------------------------------------------------------

Target "Clean" (fun _ ->
    CleanDirs [buildDir; "Sample"]
)

Target "InstallDotNetCLI" (fun _ ->
    dotnetExePath <- DotNetCli.InstallDotNetSDK dotnetcliVersion
)

Target "Restore" (fun _ ->
    appReferences
    |> Seq.iter (fun p ->
        let dir = System.IO.Path.GetDirectoryName p
        runDotnet dir "restore"
    )
)

Target "Build" (fun _ ->
    appReferences
    |> Seq.iter (fun p ->
        let dir = System.IO.Path.GetDirectoryName p
        runDotnet dir "build"
    )
)

open System.IO
let createDirIfMissing path =
    if path |> Directory.Exists |> not then
        Directory.CreateDirectory(path)
        |> ignore

let copy location from = File.Copy(from, location)

Target "SetupSample" (fun _ ->
    createDirIfMissing "Sample"
    createDirIfMissing "Sample/hotkeys"

    !! @"FishKey/bin/Debug/net461/*.dll"
    ++ @"FishKey/bin/Debug/net461/*.exe"
    |> Seq.iter (fun path ->
        copy (Path.Combine("Sample", Path.GetFileName(path))) path)

    copy "Sample/hotkeys/FishKey.Sample.dll" "FishKey.Sample/bin/Debug/net461/FishKey.Sample.dll"
    copy "Sample/hotkeys/FishKey.CSharpSample.dll" "FishKey.CSharpSample/bin/Debug/net461/FishKey.CSharpSample.dll"

    )

// --------------------------------------------------------------------------------------
// Build order
// --------------------------------------------------------------------------------------

"Clean"
  ==> "Restore"
  ==> "Build"
  ==> "SetupSample"

RunTargetOrDefault "Build"
