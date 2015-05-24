namespace DotNetMon

module Cli = 
    open System.Text.RegularExpressions
    open System.Diagnostics
    open Constants
    open IoUtils    

    type Filters = 
        | Extension of (string -> string -> bool) * string
        | Ignore of (string -> string -> bool) * string
        | Watch of string
    
    type Commands = 
        | Execution of (string -> ProcessStartInfo) * string
        | RestartServer of string * string
        | Print of string
    
    type Options = 
        | Filter of Filters
        | Command of Commands
    
    let fileExtFilter (opts : string) fileName = 
        let reg = 
            opts
            |> cleanStringArgs [| ','; ' ' |]
            |> String.concat "|"
        
        let s = sprintf "^.*\.(%s)$" reg
        Regex.IsMatch(fileName, s)
    
    let fileNameFilter wFile (fileName:string) = fileName.EndsWith(wFile)

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
              | Sext | Lext -> yield Filter(Extension(fileExtFilter, getCmndArgs args.[i + 1..]))
              | Swatch | Lwatch -> 
                let arg = args.[i + 1]
                if isFile arg then yield Filter(Extension(fileNameFilter, arg))
                else yield Filter(Watch (arg))
              | Sexec | Lexec -> yield Command(Execution(buildAction, getCmndArgs args.[i + 1..]))
              | Lserver -> yield Command(RestartServer(args.[i + 1], args.[i + 2]))
              | Shelp | Lhelp -> yield Command(Print("Help!!"))
              | Sversion | Lversion -> yield Command(Print("version 0.0.1"))              
              | _ -> () ]
