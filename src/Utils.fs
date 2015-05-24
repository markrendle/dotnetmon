namespace DotNetMon

module IoUtils = 
    open System.IO
    open System.Diagnostics
    open System
    
    let startProcess (processInfo : ProcessStartInfo) = 
        use proc = Process.Start processInfo
        proc.ErrorDataReceived.Add(fun err -> printfn "%s" err.Data)
        proc.OutputDataReceived.Add(fun data -> printfn "%s" data.Data)
        ()
    
    let processStartWrapper (program : string) (args : string) = 
        let prcss = new ProcessStartInfo()
        prcss.Arguments <- args
        prcss.FileName <- program
        prcss.CreateNoWindow <- true
        prcss.RedirectStandardInput <- false
        prcss.RedirectStandardOutput <- true
        prcss.UseShellExecute <- false
        prcss
    
    let cleanStringArgs (splitBy : char array) (args : string) = 
        args.Split(splitBy, StringSplitOptions.RemoveEmptyEntries)
        |> Array.filter (fun s -> not (String.IsNullOrWhiteSpace s))
        |> Array.map (fun s -> s.Trim())
    
    let strEquals (equalsTo : string) (str : string) = str.Equals(equalsTo, StringComparison.OrdinalIgnoreCase)

    let buildDefaults (args : string array) currentDir = 
        let checkServerparam arr = 
            if not (arr |> Array.exists (fun s -> s |> strEquals Constants.Lserver)) then 
                if File.Exists(Path.Combine(currentDir, "project.json")) then 
                    arr |> Array.append [| " --server kestrel ." |]
                else arr
            else arr
        
        let checkWatchParam arr = 
            if not (arr |> Array.exists (fun s -> s
                                                  |> strEquals Constants.Lwatch
                                                  || s |> strEquals Constants.Swatch)) then 
                arr |> Array.append [| Constants.Lwatch + " " + currentDir |]
            else arr
        
        args
        |> checkServerparam
        |> checkWatchParam
        |> String.concat " "
        |> cleanStringArgs [| ' ' |]
    
    let isFile path =
        if Directory.Exists path || File.Exists path then
            Path.HasExtension path
        else
            printfn "file or directory not found:\r\n%s" path
            false        

    let runActions fileName (filters : (string -> bool) list) (commands : ProcessStartInfo list) 
        (sideActions : (unit -> unit) list) = 
        sideActions |> List.iter (fun act -> act())
        let all = 
            filters
            |> List.map (fun apply -> apply fileName)
            |> List.forall (fun success -> success = true)
        if all then commands |> List.iter (fun proc -> startProcess proc)
