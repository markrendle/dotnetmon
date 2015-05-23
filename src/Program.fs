namespace DotNetMon

module Program = 
    open Cli
    open FSWatcher
    open System
    open ServerUtils
    open IoUtils
    
    [<EntryPoint>]
    let main argv = 
        let currentDir = Environment.CurrentDirectory
        Console.CancelKeyPress.Add(fun _ -> stopServer())
        let options = parse (buildDefaults argv currentDir)
        //OnStartActions
        for option in options do
            match option with
            | Command(Print(msg)) -> 
                printfn "%s" msg
                Environment.Exit(0)
            | Command(RestartServer(server, path)) -> startProcess (startDnx server path)
            | _ -> ()
        //Exec actions based on filters
        let runActions fileName = 
            let filters = 
                [ for o in options do
                      match o with
                      | Filter(Extension(act, args)) -> yield act fileName args
                      | _ -> () ]
            if (filters |> List.tryFind (fun x -> x = false)) = Some true then ()
            [ for o in options do
                  match o with
                  | Command(Execution(act, args)) -> yield act args
                  | Command(RestartServer(server, path)) -> 
                      stopServer()
                      yield startDnx server path
                  | _ -> () ]
            |> List.map (fun proc -> startProcess (proc))
            |> ignore
        
        let watcher = new Watcher(currentDir, (fun dirName -> runActions dirName), //Dir created
                                  (fun dirName -> runActions dirName), //Dir deleted 
                                  (fun fileName -> runActions fileName), //File created
                                  (fun fileName -> runActions fileName), //File changed
                                  (fun fileName -> runActions fileName)) //File deleted       
        watcher.Watch()
        System.Console.ReadKey |> ignore
        0
