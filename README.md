
# docopt.net F# Type Provider

An F# Type Provider for [docopt.net](https://github.com/docopt/docopt.net).

## Sample
```fsharp
  open Docopt
  
  let opts = DocOptions<@"Naval Fate.
      Usage:
        naval_fate.exe ship new <name>...
        naval_fate.exe ship <name> move <x> <y> [--speed=<kn>]
        naval_fate.exe ship shoot <x> <y>
        naval_fate.exe mine (set|remove) <x> <y> [--moored | --drifting]
        naval_fate.exe (-h | --help)
        naval_fate.exe --version

      Options:
        -h --help     Show this screen.
        --version     Show version.
        --speed=<kn>  Speed in knots [default: 10].
        --moored      Moored (anchored) mine.
        --drifting    Drifting mine.

">
```

and maybe will eventually generate the following F#
```fsharp

type MyOpts =
    new : unit -> MyOpts
    member Apply : cmdLine:string * ?help:bool * ?version:obj * ?optionsFirst:bool * ?exit:bool -> ??
    member Apply : argv:ICollection<string> * ?help:bool * ?version:obj * ?optionsFirst:bool * ?exit:bool -> ??
    member GenerateCode : doc:string -> string

```

and `docopt.net` would generate the following C# properties:
```csharp
public bool CmdShip { get { return _args["ship"].IsTrue; } }
public bool CmdNew { get { return _args["new"].IsTrue; } }
public ArrayList ArgName  { get { return _args["<name>"].AsList; } }
public bool CmdShip { get { return _args["ship"].IsTrue; } }
public ArrayList ArgName  { get { return _args["<name>"].AsList; } }
public bool CmdMove { get { return _args["move"].IsTrue; } }
public string ArgX  { get { return _args["<x>"].ToString(); } }
public string ArgY  { get { return _args["<y>"].ToString(); } }
public string OptSpeed { get { return _args["--speed"].ToString(); } }
public bool CmdShip { get { return _args["ship"].IsTrue; } }
public bool CmdShoot { get { return _args["shoot"].IsTrue; } }
public string ArgX  { get { return _args["<x>"].ToString(); } }
public string ArgY  { get { return _args["<y>"].ToString(); } }
public bool CmdMine { get { return _args["mine"].IsTrue; } }
public bool CmdSet { get { return _args["set"].IsTrue; } }
public bool CmdRemove { get { return _args["remove"].IsTrue; } }
public string ArgX  { get { return _args["<x>"].ToString(); } }
public string ArgY  { get { return _args["<y>"].ToString(); } }
public bool OptMoored { get { return _args["--moored"].IsTrue; } }
public bool OptDrifting { get { return _args["--drifting"].IsTrue; } }
public bool OptHelp { get { return _args["--help"].IsTrue; } }
public bool OptVersion { get { return _args["--version"].IsTrue; } }
```
