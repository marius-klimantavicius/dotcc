//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi Foo }

export module Foo;
export import :check;
