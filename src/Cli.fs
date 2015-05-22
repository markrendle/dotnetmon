namespace DotNetMon

module Cli = 
    open System
    open System.Text.RegularExpressions
    open System.Diagnostics
    open Constants

    type Options = 
        | Extension of string
        | Execution of string
        | PrjPath of string
        | ServerName of string
    
    type ExecInfo = 
        { Server : string
          Env : string
          ProjPath : string }
    
    let defaults = 
        { Server = "kestrel"
          Env = "dnx"
          ProjPath = "." }
    
    let parse (args : string []) = 
        let getCmndArgs (sliced : string array) = 
            let index = sliced |> Array.tryFindIndex (fun x -> x.StartsWith("-"))
            if index.IsSome then sliced.[..index.Value - 1] |> String.concat " "
            else sliced.[..sliced.Length - 1] |> String.concat " "
        seq { 
            for i in 0..args.Length - 1 do
                match args.[i].ToLower() with
                | Ext -> yield Extension(getCmndArgs args.[i + 1..])
                | Lext -> yield Extension(getCmndArgs args.[i + 1..])
                | Exec -> yield Execution(getCmndArgs args.[i + 1..])
                | Lexec -> yield Execution(getCmndArgs args.[i + 1..])
                | Lserver -> yield ServerName(args.[i + 1])
                | Lprjpath -> yield PrjPath(args.[i + 1])
                | _ -> ()
        }
    
    let cleanStringArgs (splitBy : char array) (args : string) = 
        args.Split(splitBy, StringSplitOptions.RemoveEmptyEntries)
        |> Array.filter (fun s -> not (String.IsNullOrWhiteSpace s))
        |> Array.map (fun s -> s.Trim())
    
    let getOnFileChanged (opts : string) = 
        let reg = 
            opts
            |> cleanStringArgs [| ','; ' ' |]
            |> String.concat "|"
        
        let s = sprintf "^.*\.(%s)$" reg
        fun (x : string) (action : unit -> unit) -> 
            if (Regex.IsMatch(x, s)) then action()
            else ()
    
    let processStartWrapper (program : string) (args : string) = 
        let prcss = new ProcessStartInfo()
        prcss.Arguments <- args
        prcss.FileName <- program
        prcss.CreateNoWindow <- true
        prcss.RedirectStandardInput <- false
        prcss.RedirectStandardOutput <- true
        prcss.UseShellExecute <- false
        prcss
    
    let buildAction (args : string) = 
        let arr = args |> cleanStringArgs [| ','; ' ' |]
        let progargs = arr.[1..] |> String.concat " "
        Some(processStartWrapper arr.[0] progargs)
