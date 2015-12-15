#r @"bin\Debug\TypeProviderSandbox.dll"

open TypeProviderSandbox

let x = Provider1<"FooBar">.FooBar<"year">().Value 
