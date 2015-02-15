#!/usr/bin/env fsharpi
#r @"./DocoptProvider.dll"

type theTest = Docopt.DocOptions<"usage: app --myFlag --myString=<myStringValue>">

printfn "%s" ""
printfn "%s" theTest.DocString
printfn "%s" theTest.GeneratedCSharp

theTest.Apply (System.Environment.GetCommandLineArgs())
|> printfn "%A"
