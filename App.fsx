#!/usr/bin/env fsharpi
(**
exe compile with: fsc -o:app.exe --standalone -r:docoptnetprovider app.fsx
**)
printfn "now starting app..."

#if INTERACTIVE
#r @"./DocoptProvider.dll"
#endif

// This just tests things out a little for now.
// Compile to an exe and pass "--myFlag" or whatever on the command line
// to see what happens.

type theTest = Docopt.DocOptions<"usage: App.exe --myFlag --myString=myStringValue">

printfn "The DocString: %s" theTest.DocString
printfn "The GeneratedCSharp: %s" theTest.GeneratedCSharp

printfn "executing Apply with: --myFlag --myString=myStringValue"
theTest.Apply [|"--myFlag"; "--myString=myStringValue"|]
|> printfn "output: %A"

printfn "executing Apply with command line args..."
let cmdLineArgsWithoutExe =
  System.Environment.GetCommandLineArgs()
  |> Seq.skip 1
  |> Seq.toArray

let theResult = theTest.Apply cmdLineArgsWithoutExe
theResult |> printfn "Apply returned: %+A"
printfn "The return type: %+A" (theResult.GetType())

