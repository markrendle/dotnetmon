namespace DotNetMon

module Program = 
    open Cli
    open FSWatcher
    open System.Diagnostics
    open System.IO
    open System
    open ServerUtils
    
    [<EntryPoint>]
    let main argv = 
        let options = parse (argv)
        let actionToExec : Ref<ProcessStartInfo option> = ref None
        let fileChange = ref (fun (x : string) (action : unit -> unit) -> ())
        let serverName = ref None
        let prjPath = ref None
        for option in options do
            match option with
            | Extension(fileExts) -> fileChange := getOnFileChanged fileExts
            | Execution(progs) -> actionToExec := buildAction progs
            | ServerName(name) -> serverName := Some name
            | PrjPath(path) -> prjPath := Some path
        let currentPath = Environment.CurrentDirectory
        
        let restartServer() = 
            stopServer()
            let serverOpt = serverName.Value
            let pathOpt = prjPath.Value
            if File.Exists(Path.Combine(currentPath, "project.json")) && serverOpt.IsNone && pathOpt.IsNone then 
                startServerDefaults()
            elif pathOpt.IsSome && serverOpt.IsNone then startDnxKestrel pathOpt.Value
            elif pathOpt.IsNone && serverOpt.IsSome then startDnx serverOpt.Value "."
            elif pathOpt.IsSome && serverOpt.IsSome then startDnx serverOpt.Value pathOpt.Value
            else ()
        
        let action() = 
            restartServer() //Restart the dnx server if required
            if (actionToExec.Value.IsSome) then 
                use proc = Process.Start(actionToExec.Value.Value)
                () //proc.WaitForExit() TODO: Check if is better to wait for exit :s 
        
        
        restartServer()
        let watcher = new Watcher(currentPath, (fun d -> printfn "Dir created"), //Dir created
                                  (fun d -> printfn "Dir deleted"), //Dir deleted 
                                  (fun d -> printfn "File created"), //File created
                                  (fun path -> (!) fileChange path action), //File changed
                                  (fun d -> printfn "%A" d)) //File deleted       
        
        Console.CancelKeyPress.Add(fun _ -> stopServer())
        watcher.Watch()
        System.Console.ReadKey |> ignore
        0
