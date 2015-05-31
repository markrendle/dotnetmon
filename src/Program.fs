namespace DotNetMon

module Program = 
    open Cli
    open System
    open ServerUtils
    open IoUtils
    open WatcherUtils
    
    [<EntryPoint>]
    let main argv = 
        let currentDir = Environment.CurrentDirectory
        Console.CancelKeyPress.Add(fun _ -> stopServer())
        let options = 
            argv
            |> buildDefaults currentDir
            |> parseWithFileSystem
        
        //OnStartActions
        let mustLog = ref false
        options |> List.iter (function 
                       | Command(Print(msg)) -> 
                           printfn "%s" msg
                           Environment.Exit(0)
                       | Command(RestartServer(server, path)) -> 
                           stopServer()
                           startProcess (startDnx server path)
                       | Command(Verbose) -> mustLog := true
                       | _ -> ())
        //Start the watchers
        let wOptions = 
            { currentDir = currentDir
              mustLog = mustLog.Value
              logFunction = printfn "%s" }
        options |> startWatcher wOptions
        System.Console.ReadKey |> ignore
        0
