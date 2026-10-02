//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module Foo:B;
// { dg-module-cmi {Foo:B} }

export import :A;

namespace Bob
{
export int Quux ();
}
