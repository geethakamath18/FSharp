open System
open System.IO

let bootstrapperUrl = "https://github.com/fsprojects/Paket/releases/download/5.181.1/paket.bootstrapper.exe"

let mono directory executable arguments =
    let mono = "Mono.Runtime" |> Type.GetType |> isNull |> not
    let executable, arguments =
        match mono with
        | false -> executable, arguments
        | _ -> "mono", sprintf "\"%s\" %s" executable arguments
    let info =
        ProcessStartInfo(
            executable, arguments,
            UseShellExecute = false, CreateNoWindow = true, WorkingDirectory = directory)
    let proc = Process.Start(info)
    proc.WaitForExit()
    match proc.ExitCode with | 0 -> () | c -> failwithf "Execution failed with error code %d" c

let restore () =
    let root = __SOURCE_DIRECTORY__
    let inline pathOf p = Path.Combine (root, p)
    let inline exists p = File.Exists p
    let paket = pathOf ".paket/paket.exe"
    let lock = pathOf "paket.lock"
    if paket |> exists |> not then
        Directory.CreateDirectory(Path.GetDirectoryName(paket)) |> ignore
        use client = new Net.WebClient()
        client.DownloadFile(bootstrapperUrl, paket)
    mono root paket (if lock |> exists then "restore" else "install")

restore ()