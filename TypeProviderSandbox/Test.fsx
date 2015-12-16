#r @"bin\Debug\TypeProviderSandbox.dll"

open TypeProviderSandbox

let x = Provider1<"Foo">.Foo<"Bar">().Bar<"Baz">()