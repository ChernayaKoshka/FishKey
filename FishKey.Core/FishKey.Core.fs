namespace FishKey.Core

[<AutoOpen>]
module Types =
    open System
    open Gma.System.MouseKeyHook

    [<AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)>]
    type HotkeyAttribute(combination) =
        inherit Attribute()
        member val KeyTrigger = Combination.FromString(combination)
