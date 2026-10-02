//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts -fdump-lang-module" }

module Foo;
// { dg-final { scan-lang-dump "Starting module Foo" "module" } }
