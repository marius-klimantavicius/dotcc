//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module foo:baz;
// { dg-module-cmi foo:baz }

int foo (int);
