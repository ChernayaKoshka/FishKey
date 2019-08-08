namespace FishKey.Core

[<AutoOpen>]
module Types =
    open System
    open Gma.System.MouseKeyHook

    [<AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)>]
    type HotkeyAttribute(combination, titlePattern) =
        inherit Attribute()

        new(combination) =
            HotkeyAttribute(combination, String.Empty)

        member val KeyTrigger = Combination.FromString(combination)
        member val TitlePattern = titlePattern
