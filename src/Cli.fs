namespace DotNetMon

module Cli = 
    open System.Text.RegularExpressions
    open System.Diagnostics
    open Constants
    open IoUtils
    
    type Filters = 
        | Extension of (string -> string -> bool) * string
    
    type Commands = 
        | Execution of (string -> ProcessStartInfo) * string
        | RestartServer of string * string
        | Print of string
    
    type Options = 
        | Filter of Filters
        | Command of Commands
    
    let getFilter fileName (opts : string) = 
        let reg = 
            opts
            |> cleanStringArgs [| ','; ' ' |]
            |> String.concat "|"
        
        let s = sprintf "^.*\.(%s)$" reg
        Regex.IsMatch(fileName, s)
    
    let buildAction (args : string) = 
        let arr = args |> cleanStringArgs [| ','; ' ' |]
        let progargs = arr.[1..] |> String.concat " "
        processStartWrapper arr.[0] progargs
    
    let parse (args : string []) = 
        let getCmndArgs (sliced : string array) = 
            let index = sliced |> Array.tryFindIndex (fun x -> x.StartsWith("-"))
            if index.IsSome then sliced.[..index.Value - 1] |> String.concat " "
            else sliced.[..sliced.Length - 1] |> String.concat " "
        [ for i in 0..args.Length - 1 do
              match args.[i].ToLower() with
              | Ext | Lext -> yield Filter(Extension(getFilter, getCmndArgs args.[i + 1..]))
              | Exec | Lexec -> yield Command(Execution(buildAction, getCmndArgs args.[i + 1..]))
              | Lserver -> yield Command(RestartServer(args.[i + 1], args.[i + 2]))
              | Shelp | Lhelp -> yield Command(Print("Help!!"))
              | Lversion | Sversion -> yield Command(Print("version 0.0.1"))
              | _ -> () ]
