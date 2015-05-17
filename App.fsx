#!/usr/bin/env fsharpi
#r @"./DocoptProvider.dll"
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

theTest.Apply cmdLineArgsWithoutExe
|> printfn "output: %A"
