#r @"bin\Debug\TypeProviderSandbox.dll"

open TypeProviderSandbox

let x = Provider1<"FooBar">.FooBar<"now">().Value 
