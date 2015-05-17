module Docopt
(**
compile with: fsc -a -o:docoptprovider.dll -r:docoptnet paket-files/fsprojects/FSharp.TypeProviders.StarterPack/src/ProvidedTypes.fs
**)

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
open DocoptNet
open System.Collections.Generic

// Disable incomplete matches warning
// Incomplete matches are used extensively within this file
// to simplify the code
#nowarn "0025"

[<AutoOpen>]
module internal Provide =
    let inline xmlComment comment providedMember =
      (^a : (member AddXmlDocDelayed : (unit -> string) -> unit) providedMember, (fun () -> comment))
      providedMember

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

                printfn "now inside type provider..."

                let g = ProvidedTypeDefinition(
                            asm,
                            ns,
                            typeName,
                            Some typeof<obj>,
                            IsErased = false)

                ProvidedProperty("DocString", typeof<string>, IsStatic=true, GetterCode = (fun _ -> <@@ docoptDocString @@>))
                |> xmlComment "Gets the full doc string."
                |> g.AddMember

                ProvidedProperty("GeneratedCSharp", typeof<string>, IsStatic=true, GetterCode = (fun _ -> <@@ generatedCSharp @@>))
                |> xmlComment "[debug] Gets the C# code that would be generated by DocOpt.NET"
                |> g.AddMember

                let optInstanceType = ProvidedTypeDefinition(
                                        asm, ns, "MyOptions", baseType = Some typeof<obj>,
                                        HideObjectMethods = true, IsErased=false)
                
                let theValuesField = ProvidedField("theValues", (typeof<IDictionary<string, ValueObject>>))
                optInstanceType.AddMember theValuesField

                let optInstanceParamDocoptValues = ProvidedParameter("values", typeof<IDictionary<string, ValueObject>>)

                let optInstanceCtor = ProvidedConstructor([optInstanceParamDocoptValues],
                                                          InvokeCode=(fun _ -> <@@ () @@> ))

                optInstanceType.AddMember optInstanceCtor
                
                docopt.GetNodes(docoptDocString)
                |> Seq.map (fun m -> printfn "found parsed docopt node: %A" m; m)
                |> Seq.distinctBy id
                |> Seq.map (function
                    | :? DocoptNet.CommandNode as cmd ->
                        ProvidedProperty(cmd.Name, typeof<bool>, GetterCode = (fun args -> <@@ Expr.FieldGet(%%args.[0], theValuesField) @@>))
                    | :? DocoptNet.OptionNode as opt ->
                        ProvidedProperty(opt.Name, typeof<bool>, GetterCode = (fun _ -> <@@ false @@>))
                    | :? DocoptNet.ArgumentNode as arg ->
                        ProvidedProperty(arg.Name, typeof<string>, GetterCode = (fun _ -> <@@ "test" @@>))
                    | n -> failwithf "unexpected node type %A" (n.GetType())
                )
                |> Seq.map (fun m -> printfn "generated %s property: %s" optInstanceType.Name (m.Name); m)
                |> Seq.iter optInstanceType.AddMember
                
                tempAsm.AddTypes([optInstanceType])

                ProvidedMethod("Apply", [ ProvidedParameter("cmdLineArgs", typeof<string[]>)], typeof<obj>, IsStaticMethod=true, InvokeCode =
                    (fun [ cmdLineArgs ] ->
                        <@@
                            let argv = (%%cmdLineArgs:string[])
                            printfn "inside DocoptProvider.Apply() now, got argv: %A" argv
                            let d = DocoptNet.Docopt()
                            let values : IDictionary<string, DocoptNet.ValueObject> =
                                d.Apply (docoptDocString, argv, version="1", optionsFirst=true, exit=false)
                            printfn "still in DocoptProvider.Apply(), got docopt values: %A" values
                            
                            let allTypes = Assembly.GetExecutingAssembly().GetExportedTypes()
                            let typesExclFSharp =
                                allTypes
                                |> Seq.filter (fun t -> not (t.FullName.StartsWith("Microsoft.FSharp")))
                                |> Seq.toArray
                            printfn "types in executing assembly: %A" typesExclFSharp
                            
                            let myOptionsTypeCtor =
                                typesExclFSharp |> Seq.find (fun t -> t.FullName = "Docopt.MyOptions")
                                |> (fun myOptionsType -> myOptionsType.GetConstructors() |> Seq.exactlyOne)

                            myOptionsTypeCtor.Invoke([| values |])
                        @@>))
                |> xmlComment "Parses the command line arguments."
                |> g.AddMember

                tempAsm.AddTypes [g]
                g
            )

    do
        this.RegisterRuntimeAssemblyLocationAsProbingFolder cfg
        tempAsm.AddTypes [t]
        this.AddNamespace(ns, [t])

[<assembly:TypeProviderAssembly>]
do ()
