//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
export module Baz;
// { dg-module-cmi "Baz" }

void Quux (void);

export void Bar (void);

void Foo (void);
