namespace DotNetMon

module WatcherUtils = 
    open Cli
    open FSWatcher
    open ServerUtils
    open IoUtils
    open System.IO
    
    type watcherOptions = 
        { currentDir : string
          mustLog : bool
          logFunction : string -> unit }
    
    let startWatcher (watchOpts : watcherOptions) cliOpts = 
        let filters = 
            [ for o in cliOpts do
                  match o with
                  | Filter(File(filter)) -> yield filter
                  | _ -> () ]
        
        let commands = 
            [ for o in cliOpts do
                  match o with
                  | Command(Execution(startInfo)) -> yield startInfo
                  | Command(RestartServer(server, path)) -> yield startDnx server path
                  | _ -> () ]
        
        let runActionsWrapper event fileName = 
            if watchOpts.mustLog then watchOpts.logFunction (sprintf "%s\r\n%s" event fileName)
            runActions fileName filters commands [ stopServer ]
        
        let buildWatcher dir = new Watcher(dir, //whatch
                                           (fun dirName -> dirName |> runActionsWrapper "Directory created"), //Dir created
                                           (fun dirName -> dirName |> runActionsWrapper "Directory deleted"), //Dir deleted 
                                           (fun fileName -> fileName |> runActionsWrapper "File created"), //File created
                                           (fun fileName -> fileName |> runActionsWrapper "File changed"), //File changed
                                           (fun fileName -> fileName |> runActionsWrapper "File deleted")) //File deleted  
        cliOpts |> List.iter (function 
                       | Filter(Watch dir) -> 
                           let w = buildWatcher (Path.Combine(watchOpts.currentDir, dir))
                           w.Watch()
                       | _ -> ())
