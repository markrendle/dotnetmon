# DotNetMon

DotNetMon is an "in-process file watcher" for .NET

It's like nodemon but for use in .NET and written in F#
## Instalation

At this time there is no NuGet package or any other way of install, build from source and add the exec file in your path.

##Usage 
DotNetMon wraps the starting of web server, so you can run dotnetmon in the project directory:

```sh
> dotnetmon -e cs,cshtml
```

###Defaults
By default dotnetmon runs kestrel in "." path.

###Runing web
use the ```--server``` option for select a server from your project.json file:
```sh
> dotnetmon -e cs,cshtml --server web
```

###Running other processes

DotNetMon can also be used to execute other programs:

```sh
> dotnetmon -e txt,py --exec python app.py
```

##TODO
This is a proof of concept, there is no tests yet and I'm still learning F#, so the code, and the solution itself, maybe wrong or complex, so, Forks and Pull Requests appreciated!
* Add tests
* Add all options avalible in nodemon 

## Why
The other day I saw this jeremydmiller's [tweet](https://twitter.com/jeremydmiller/status/600729966783922177) and I tought it would be great to build one.

###hey, do you know kmon?

Yeah, I know kmon "solves" the problem, but is only a wrapper around nodemon and looks like don't will remove [this dependency](https://github.com/henriksen/kmon/issues/3)

###hey, do you know the ```--watch``` option?

Yeah, I know you can use ```--watch kestrel``` but this only [watch for file change](https://github.com/aspnet/dnx/blob/4c968ac60d78cf621c5f338538a5f2fccdf8d163/src/Microsoft.Framework.Runtime/DefaultHost.cs#L136) and stop the server... anithing more, and looks like is [not working on OSX](https://github.com/henriksen/kmon/issues/3#issuecomment-66335661)

##Tech
DotNetMon uses the [FSWatcher]("https://github.com/acken/FSWatcher") library for watch file changes. FSWatcher looks as the best option written in C# and working well on Mono. 

At this time is using a dll reference because [there is no nuget package](https://github.com/acken/FSWatcher/issues/2)