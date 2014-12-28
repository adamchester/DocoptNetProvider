#!/usr/bin/env fsharpi
#r @"./DocoptProvider.dll"

type theTest = Docopt.DocOptions<"usage: blah --myFlag --myString=<myStringValue>">

printfn "%s" theTest.DocString
printfn "%s" theTest.GeneratedCSharp


