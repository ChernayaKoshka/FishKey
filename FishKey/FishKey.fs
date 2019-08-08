module FishKey

open System
open System.IO
open System.Collections.Generic
open System.Reflection
open System.Windows.Forms

open Gma.System.MouseKeyHook

open FishKey.Core
open PInvoke
open System.Text.RegularExpressions

let getActiveTitle() =
    User32.GetForegroundWindow()
    |> User32.GetWindowText

let createDirIfMissing dir =
    if dir |> Directory.Exists |> not then
        Directory.CreateDirectory(dir) |> ignore

let loadHotkeysFromAssembly path =
    let hotkeys =
        Assembly
            .LoadFrom(path)
            .GetTypes()
        |> Seq.collect (fun t ->
            t.GetMethods())
        |> Seq.choose (fun t ->
            let hotkeyAttribute =
                t.GetCustomAttributes()
                |> Seq.tryFind (fun att ->
                    att.GetType() = typedefof<HotkeyAttribute>)
            hotkeyAttribute
            |> Option.map (fun att -> (att :?> HotkeyAttribute, t)))
        |> Seq.map (fun (hotkey, method) ->
            if method.IsStatic then
                (hotkey.KeyTrigger,
                    (fun () ->
                        try
                            if Regex.IsMatch(getActiveTitle(), hotkey.TitlePattern) then
                                method.Invoke(null, null)
                                |> ignore
                        with
                        | ex -> printfn "%A" ex))
                |> Ok
            else
                Error (sprintf "%A: Functions marked with the Hotkey attribute must be static." hotkey))
        |> Seq.choose (function Ok hotkey -> Some hotkey | Error err -> Console.WriteLine(err); None)
    printfn "Loaded %A from %s" hotkeys (Path.GetFileName(path))
    hotkeys

let hotkeysToDictionary =
    Seq.fold (fun (dict : Dictionary<Combination, Action>) (hotkey, method) ->
        dict.Add(hotkey, Action(method))
        dict) (new Dictionary<Combination, Action>())

let extensionFilter str =
    (str = ".dll" || str = ".exe")

[<EntryPoint>]
let main argv =
    #if !DEBUG
    Console.Hide()
    |> ignore
    #endif

    createDirIfMissing "hotkeys"

    printfn "Loading hotkeys from 'hotkeys' directory."
    Directory.GetFiles("hotkeys")
    |> Seq.filter (Path.GetExtension >> extensionFilter)
    |> Seq.collect loadHotkeysFromAssembly
    |> hotkeysToDictionary
    |> Hook.GlobalEvents().OnCombination
    printfn "Done loading hotkeys from 'hotkeys' directory."

    if argv.Length = 1 && argv.[0] = "debugmode" then
        Console.Show() |> ignore
        printfn "debugmode set, press any key to continue..."
        Console.ReadLine() |> ignore

    Application.Run(new ApplicationContext())
    0 // return an integer exit code
