namespace TypeProviderSandbox

open System
open Microsoft.FSharp.Core.CompilerServices
open ProviderImplementation.ProvidedTypes
open System.Reflection

[<TypeProvider>]
type Provider1 (_config: TypeProviderConfig) as self =
    inherit TypeProviderForNamespaces()
    let ns = "TypeProviderSandbox"
    let asm = Assembly.GetExecutingAssembly()
    let rootType = ProvidedTypeDefinition(asm, ns, "Provider1", Some typeof<obj>, HideObjectMethods = true)
    let rootParams = [ProvidedStaticParameter("GeneratingMethodName", typeof<string>)]
    do rootType.DefineStaticParameters(rootParams, fun typeName args ->
        let rootTy = ProvidedTypeDefinition(asm, ns, typeName, Some typeof<obj>, HideObjectMethods = true)
                
        let rec createMethod (parentTy: ProvidedTypeDefinition) name isStatic =
            let innerTy = ProvidedTypeDefinition("InnerType", Some typeof<obj>, HideObjectMethods = true)
            let m = ProvidedMethod(name, [], innerTy, IsStaticMethod = isStatic)
            let methodParams = [ProvidedStaticParameter("WhatToReturn", typeof<string>)]
            m.DefineStaticParameters(methodParams, fun methodName methodArgs ->
                innerTy.AddMemberDelayed (fun _ -> createMethod innerTy (methodArgs.[0] :?> string) false)
                let m2 = ProvidedMethod(methodName, [], innerTy, IsStaticMethod = isStatic, InvokeCode = fun _ -> <@@ null @@>)
                parentTy.AddMember m2
                m2)
            parentTy.AddMember innerTy
            parentTy.AddMember m
            m
                    
        createMethod rootTy (args.[0] :?> string) true |> ignore
        rootTy)

    do self.AddNamespace(ns, [rootType])

[<assembly:TypeProviderAssembly>]
do ()
