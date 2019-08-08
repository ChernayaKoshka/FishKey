[<RequireQualifiedAccess>]
module Console

module internal PInvoke =
    open System.Runtime.InteropServices
    [<DllImport("user32.dll")>]
    extern bool ShowWindow(nativeint hWnd, int flags)

    [<DllImport("kernel32.dll")>]
    extern nativeint GetConsoleWindow()
open PInvoke

let private showWindowWithFlags flags =
    ShowWindow(GetConsoleWindow(), flags)

let Hide() =
    ShowWindow(GetConsoleWindow(), 0)

let Show() =
    ShowWindow(GetConsoleWindow(), 9)