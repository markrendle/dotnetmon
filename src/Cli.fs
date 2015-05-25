﻿namespace DotNetMon

module Cli = 
    open System.Text.RegularExpressions
    open System.Diagnostics
    open Constants
    open IoUtils
    
    type Filters = 
        | File of (string -> string -> bool) * string
        | Watch of string
    
    type Commands = 
        | Execution of (string -> ProcessStartInfo) * string
        | RestartServer of string * string
        | Print of string
    
    type Options = 
        | Filter of Filters
        | Command of Commands
    
    let parse (args : string []) = 
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
        
        let watchFileFilter (expcFile:string) givenFile = 
            let pattern = sprintf "^%s$" (Regex.Escape(expcFile))
            Regex.IsMatch(givenFile, pattern)
        
        let ignoreFileFilter expcFile givenFile = not (watchFileFilter expcFile givenFile)
        
        let ignoreDirFilter (expcDir:string) givenDir =             
            let pattern = sprintf "^%s" (Regex.Escape(expcDir))            
            not (Regex.IsMatch(givenDir, pattern))
            
        [ for i in 0..args.Length - 1 do
              match args.[i].ToLower() with
              | Sext | Lext -> yield Filter(File(fileExtFilter, getCmndArgs args.[i + 1..]))
              | Swatch | Lwatch -> 
                  let arg = args.[i + 1]
                  if isFile arg then yield Filter(File(watchFileFilter, arg))
                  else yield Filter(Watch(arg))
              | Signore | Lignore -> 
                  let arg = args.[i + 1]
                  if isFile arg then yield Filter(File(ignoreFileFilter, arg))
                  else yield Filter(File(ignoreDirFilter, arg))
              | Sexec | Lexec -> yield Command(Execution(buildAction, getCmndArgs args.[i + 1..]))
              | Lserver -> yield Command(RestartServer(args.[i + 1], args.[i + 2]))
              | Shelp | Lhelp -> yield Command(Print("Help!!"))
              | Sversion | Lversion -> yield Command(Print("version 0.0.1"))
              | _ -> () ]
