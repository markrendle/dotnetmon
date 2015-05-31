namespace DotNetMon.Test

open System
open NUnit.Framework
open DotNetMon
open Swensen.Unquote

[<TestFixture>]
module CliTest = 
    open Cli
    
    let mockIsFile (args : string) = IoUtils.Success (not (args.EndsWith @"\"))
    let parseWithMockFile args = parse mockIsFile (fun () -> "mock version") args
    
    [<TestCase([| "--ext"; "js" |], "foo.js")>]
    [<TestCase([| "-e"; "js" |], "foo.js")>]
    let ``When pass an -e command, should get Options list with a Filter File otion`` (args, fileName) = 
        let result = parseWithMockFile args
        
        let is = 
            match result.Head with
            | Filter(File(func)) -> func fileName
            | _ -> false
        test <@ result.Length = 1 @>
        test <@ is = true @>
    
    [<TestCase([| "--watch"; @"Foo\Bar\" |])>]
    [<TestCase([| "-w"; @"Foo\Bar\" |])>]
    let ``When pass an -w command with a folder path, should get Options list with a File Watch otion`` (args) = 
        let result = parseWithMockFile args
        
        let is = 
            match result.Head with
            | Filter(Watch(dirName)) -> dirName
            | _ -> String.Empty
        test <@ result.Length = 1 @>
        test <@ is = @"Foo\Bar\" @>
    
    [<TestCase([| "--watch"; @"Foo\Bar\foo.cs" |], @"Foo\Bar\foo.cs")>]
    [<TestCase([| "-w"; @"Foo\Bar\foo.cs" |], @"Foo\Bar\foo.cs")>]
    let ``When pass an -w command with a file path, should get Options list with a Filter File otion`` (args, fileName) = 
        let result = parseWithMockFile args
        
        let is = 
            match result.Head with
            | Filter(File(func)) -> func fileName
            | _ -> false
        test <@ result.Length = 1 @>
        test <@ is @>
    
    [<TestCase([| "--ignore"; @"Foo\Bar\" |], @"Foo\Bar\foo.cs")>]
    [<TestCase([| "-i"; @"Foo\Bar\" |], @"Foo\Bar\foo.cs")>]
    let ``When pass an -i command with a file path, should get Options list with a Filter File otion`` (args, fileName) = 
        let result = parseWithMockFile args
        
        let mustWatch = 
            match result.Head with
            | Filter(File(ignore)) -> ignore fileName
            | _ -> false
        test <@ result.Length = 1 @>
        test <@ mustWatch = false @>
    
    [<TestCase([| "--exec"; "python hello.py" |])>]
    [<TestCase([| "-x"; "python hello.py" |])>]
    let ``When pass an exec command, should get Options list with a Comand Execution option`` (args) = 
        let result = parseWithMockFile args
        
        let proccessStartInfo = 
            match result.Head with
            | Command(Execution(startInfo)) -> startInfo
            | _ -> null
        test <@ result.Length = 1 @>
        test <@ proccessStartInfo.FileName = "python" @>
        test <@ proccessStartInfo.Arguments = "hello.py" @>
    
    [<TestCase([| "--help" |])>]
    [<TestCase([| "-h" |])>]
    let ``When pass a help command, should get Options list with a Comand Print option`` (args) = 
        let result = parseWithMockFile args
        
        let msg = 
            match result.Head with
            | Command(Print(msg)) -> msg
            | _ -> null
        test <@ result.Length = 1 @>
        test <@ not (String.IsNullOrEmpty msg) @>
