#r @"bin\Debug\TypeProviderSandbox.dll"

open TypeProviderSandbox

let x = Provider1<"Fo">.FooBar<"now">().Value 
