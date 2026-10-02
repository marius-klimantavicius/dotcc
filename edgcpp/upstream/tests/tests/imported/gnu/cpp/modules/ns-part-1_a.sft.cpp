//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module Foo:A;
// { dg-module-cmi {Foo:A} }

namespace Bob
{
export int Random ();
}
