module FishKey

open System
open System.Collections.Generic
open System.Reflection
open System.Windows.Forms

open Gma.System.MouseKeyHook

[<AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)>]
type HotkeyAttribute(keys) =
    inherit Attribute()
    member val KeyTrigger = Combination.FromString(keys)

[<Hotkey("Control+S")>]
let myFunc() =
    printfn "Executed!"

[<EntryPoint>]
let main argv =
    Assembly
        .GetExecutingAssembly()
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
                    method.Invoke(null, null)
                    |> ignore))
            |> Ok
        else
            Error (sprintf "%A: Functions marked with the Hotkey attribute must be static." hotkey))
    |> Seq.choose (function Ok hotkey -> Some hotkey | Error err -> Console.WriteLine(err); None)
    |> Seq.fold (fun (dict : Dictionary<Combination, Action>) (hotkey, method) ->
        dict.Add(hotkey, Action(method))
        dict) (new Dictionary<Combination, Action>())
    |> Hook.GlobalEvents().OnCombination
    Application.Run(new ApplicationContext())
    0 // return an integer exit code