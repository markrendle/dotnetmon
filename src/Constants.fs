namespace DotNetMon

module Constants = 
    [<Literal>]
    let Sext = "-e"
    
    [<Literal>]
    let Lext = "--ext"
    
    [<Literal>]
    let Sexec = "-x"
    
    [<Literal>]
    let Lexec = "--exec"
    
    [<Literal>]
    let Lserver = "--server"

    [<Literal>]
    let Shelp = "-h"

    [<Literal>]
    let Lhelp = "--help"

    [<Literal>]
    let Lversion = "--version"

    [<Literal>]
    let Sversion = "-v"

    [<Literal>]
    let Swatch = "-w"

    [<Literal>]
    let Lwatch = "--watch"
    
    [<Literal>]
    let Signore = "-i"

    [<Literal>]
    let Lignore = "--ignore"

    [<Literal>]
    let Lverbose = "--verbose"

    let HelpMessage = 
        "Usage: dotnetmon [options]\r\n
Options:\r\n
-e, --ext ................ extensions to look for, ie. js,jade,hbs.
-x, --exec app ........... execute script with \"app\", ie. -x \"python -v\".
-w, --watch dir........... watch directory \"dir\" or files. use once for 
                                   each directory or file to watch.
-i, --ignore ............. ignore specific files or directories. 
--server server args........specify the server to use and their arguments. 
                                    servername must be in the project.json file
--verbose ................ show detail on what is causing restarts.
-v, --version ............ current nodemon version.
-h, --help ............... you're looking at it."