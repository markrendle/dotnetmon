namespace DotNetMon

module Cli = 
    open System.Text.RegularExpressions
    open System.Diagnostics
    open Constants
    open IoUtils
    
    type Filters = 
        | File of (string -> bool)
        | Watch of string
    
    type Commands = 
        | Execution of ProcessStartInfo
        | RestartServer of string * string
        | Verbose
        | Print of string
    
    type Options = 
        | Filter of Filters
        | Command of Commands
    
    let parse (isFile : (string -> Result<bool,string>)) (getVersion:(unit -> string)) (args : string []) = 
        let getCmndArgs (sliced : string array) = 
            let index = sliced |> Array.tryFindIndex (fun x -> x.StartsWith("-"))
            if index.IsSome then sliced.[..index.Value - 1] |> String.concat " "
            else sliced.[..sliced.Length - 1] |> String.concat " "
        
        let buildAction (args : string) = 
            let arr = args |> cleanStringArgs [| ','; ' ' |]
            let progargs = arr.[1..] |> String.concat " "
            processStartWrapper arr.[0] progargs
        
        let fileExtFilter (opts : string) fileName = 
            let reg = 
                opts
                |> cleanStringArgs [| ','; ' ' |]
                |> String.concat "|"
            
            let pattern = sprintf "^.*\.(%s)$" reg
            Regex.IsMatch(fileName, pattern)
        
        let watchFileFilter (expcFile : string) givenFile = 
            let pattern = sprintf "^%s$" (Regex.Escape(expcFile))
            Regex.IsMatch(givenFile, pattern)
        
        let ignoreFileFilter expcFile givenFile = not (watchFileFilter expcFile givenFile)
        
        let ignoreDirFilter (expcDir : string) givenDir = 
            let pattern = sprintf "^%s" (Regex.Escape(expcDir))
            not (Regex.IsMatch(givenDir, pattern))
        [ for i in 0..args.Length - 1 do
              match args.[i].ToLower() with
              | Sext | Lext -> yield Filter(File(fileExtFilter (getCmndArgs args.[i + 1..])))
              | Swatch | Lwatch -> 
                  let arg = args.[i + 1]
                  match isFile arg with
                  | Success true -> yield Filter(File(watchFileFilter arg))
                  | Success false -> yield Filter(Watch(arg))
                  | Failure (msg) -> yield Command(Print(sprintf "Error: %s" msg))                  
              | Signore | Lignore -> 
                  let arg = args.[i + 1]
                  match isFile arg with
                  |Success true -> yield Filter(File(ignoreFileFilter arg))
                  |Success false -> yield Filter(File(ignoreDirFilter arg))
                  | Failure (msg) -> yield Command(Print(sprintf "Error: %s" msg))                 
              | Sexec | Lexec -> yield Command(Execution(buildAction (getCmndArgs args.[i + 1..])))
              | Lserver -> yield Command(RestartServer(args.[i + 1], args.[i + 2]))
              | Lverbose -> yield Command(Verbose)
              | Shelp | Lhelp -> yield Command(Print("Help!!"))
              | Sversion | Lversion -> yield Command(Print(sprintf "v%s"(getVersion())))
              | _ -> () ]
    
    let parseWithFileSystem (args : string []) = parse isFile getAssemblyVersion args
