module Samples

open System
open System.Windows.Forms

open FishKey.Core

[<Hotkey("Alt+I")>]
let testFunc() =
    MessageBox.Show("Alt+I was pressed anywhere!")

[<Hotkey("Alt+S", "^Untitled")>]
let untitledTestFunc() =
    MessageBox.Show("Alt+S was used in a window that matched the criteria '^Untitled'")

