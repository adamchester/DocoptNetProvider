#!/usr/bin/env fsharpi
#r @"./DocoptProvider.dll"

type theTest = Docopt.DocOptions<"usage: app --myFlag --myString=<myStringValue>">

printfn "%s" theTest.DocString
printfn "%s" theTest.GeneratedCSharp


