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
    
    let buildDefaults (args : string array) currentDir = 
        let strArgs = args |> String.concat " "
        if strArgs.Contains("--server") then strArgs |> cleanStringArgs [| ' ' |]
        else if File.Exists(Path.Combine(currentDir, "project.json")) then 
            strArgs + " --server kestrel ." |> cleanStringArgs [| ' ' |]
        else strArgs |> cleanStringArgs [| ' ' |]
