module Docopt

#if INTERACTIVE
#I @"packages/docopt.net/lib/net40"
#r "DocoptNet.dll"
#load "paket-files/fsprojects/FSharp.TypeProviders.StarterPack/src/ProvidedTypes.fsi"
#load "paket-files/fsprojects/FSharp.TypeProviders.StarterPack/src/ProvidedTypes.fs"
#endif

open System.Reflection
open ProviderImplementation.ProvidedTypes
open Microsoft.FSharp.Core.CompilerServices
open Microsoft.FSharp.Quotations

[<TypeProvider>]
type DocoptProvider (cfg : TypeProviderConfig) as this =
    inherit TypeProviderForNamespaces ()

    let docopt = DocoptNet.Docopt()
    let ns = "Docopt"
    let asm = Assembly.GetExecutingAssembly()
    let tempAsmPath = System.IO.Path.ChangeExtension(System.IO.Path.GetTempFileName(), ".dll")
    let tempAsm = ProvidedAssembly tempAsmPath

    let t = ProvidedTypeDefinition(asm, ns, "DocOptions", Some typeof<obj>, IsErased = false)
    let parameters = [ProvidedStaticParameter("DocString", typeof<string>)]

    do
        t.DefineStaticParameters(
            parameters,
            fun typeName args ->
                let docoptDocString = args.[0] :?> string
                let generatedCSharp = docopt.GenerateCode(docoptDocString)
                printfn "generatedCSharp: \n%s" generatedCSharp

                let g = ProvidedTypeDefinition(
                            asm, 
                            ns, 
                            typeName, 
                            Some typeof<obj>, 
                            IsErased = false)

                g.AddMember (ProvidedProperty("DocString", typeof<string>, IsStatic=true, GetterCode= (fun _ -> <@@ docoptDocString @@>)))
                g.AddMember (ProvidedProperty("GeneratedCSharp", typeof<string>, IsStatic=true, GetterCode= (fun _ -> <@@ generatedCSharp @@>)))
                tempAsm.AddTypes [g]                
                g
            )

    do
        this.RegisterRuntimeAssemblyLocationAsProbingFolder cfg
        tempAsm.AddTypes [t]
        this.AddNamespace(ns, [t])

[<assembly:TypeProviderAssembly>]
do ()

