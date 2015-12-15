namespace TypeProviderSandbox

open System
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open System.Reflection
open System.Runtime.Caching

[<TypeProvider>]
type Provider1 (_config: TypeProviderConfig) as self =
    inherit TypeProviderForNamespaces()
    let ns = "TypeProviderSandbox"
    let asm = Assembly.GetExecutingAssembly()
    let rootType = ProvidedTypeDefinition(asm, ns, "Provider1", Some typeof<obj>, HideObjectMethods = true)
    let methodCaches = new MethodCaches()
    let rootTyCache = new MemoryCache("Provider1")
    let rootParams = [ProvidedStaticParameter("GeneratingMethodName", typeof<string>)]
    do rootType.DefineStaticParameters(rootParams, fun typeName args ->
        lazy
            let rootTy = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>, HideObjectMethods = true)
            let innerTy = ProvidedTypeDefinition("InnerType", Some typeof<obj>, HideObjectMethods = true)
            let methodName = args.[0] :?> string
            if methodName.Length < 3 then failwith "Method name must be at least three letters long"
            let m = ProvidedMethod(methodName, [], innerTy, IsStaticMethod = true)
            let methodParams = [ProvidedStaticParameter("WhatToReturn", typeof<string>)]
            m.DefineStaticParameters(methodParams, fun methodName methodArgs ->
                lazy
                    let valueProp =
                        match methodArgs.[0] :?> string with
                        | "now" -> ProvidedProperty("Value", typeof<DateTime>, IsStatic = false, GetterCode = fun _ -> <@@ DateTime.Now @@>)
                        | "year" -> ProvidedProperty("Value", typeof<int>, IsStatic = false, GetterCode = fun _ -> <@@ DateTime.Now.Year @@>)
                        | _ -> failwith "WhatToReturn must be either 'now' or 'year'"
                    
                    innerTy.AddMember valueProp
                    let m2 = ProvidedMethod(methodName, [], innerTy, IsStaticMethod = true, InvokeCode = fun _ -> <@@ obj() @@>)
                    rootTy.AddMember m2
                    m2
                |> (methodCaches.GetOrAdd "Method").GetOrAdd methodName
            )
            
            rootTy.AddMember innerTy
            rootTy.AddMember m
            rootTy
        |> rootTyCache.GetOrAdd typeName)

    do self.AddNamespace(ns, [rootType])
       self.Disposing.Add <| fun _ -> (methodCaches :> IDisposable).Dispose()

[<assembly:TypeProviderAssembly>]
do ()
