namespace DotNetMon

module Program = 
    open Cli
    open FSWatcher
    open System
    open ServerUtils
    open IoUtils
    open System.IO
    open System.Diagnostics
    
    [<EntryPoint>]
    let main argv = 
        let currentDir = Environment.CurrentDirectory
        Console.CancelKeyPress.Add(fun _ -> stopServer())
        let options = parse (buildDefaults argv currentDir)
        
        let filters = 
            [ for o in options do
                  match o with
                  | Filter(Extension(act, args)) -> yield act args
                  | _ -> () ]
        
        let commands = 
            [ for o in options do
                  match o with
                  | Command(Execution(act, args)) -> yield act args
                  | Command(RestartServer(server, path)) -> yield startDnx server path
                  | _ -> () ]

        let runActionsWrapper fileName = runActions fileName filters commands [stopServer]
        let buildWatcher dir = 
            // TODO: Check if dirOrFile exists
            new Watcher(dir, //whatch
                        (fun dirName -> runActionsWrapper dirName), //Dir created
                        (fun dirName -> runActionsWrapper dirName), //Dir deleted 
                        (fun fileName -> runActionsWrapper fileName), //File created
                        (fun fileName -> runActionsWrapper fileName), //File changed
                        (fun fileName -> runActionsWrapper fileName)) //File deleted  
        //OnStartActions
        options |> List.iter (function 
                       | Command(Print(msg)) -> 
                           printfn "%s" msg
                           Environment.Exit(0)
                       | Command(RestartServer(server, path)) -> 
                           stopServer()
                           startProcess (startDnx server path)
                       | _ -> ())
        //Start the watchers        
        options |> List.iter (function 
                       | Filter(Watch dir) -> 
                           let w = buildWatcher (Path.Combine(currentDir, dir))
                           w.Watch()
                       | _ -> ())
        System.Console.ReadKey |> ignore
        0
