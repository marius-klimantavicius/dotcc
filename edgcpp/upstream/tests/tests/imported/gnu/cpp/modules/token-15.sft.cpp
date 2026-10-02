//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

#define bob() fred
export module foo.bar:baz.bob;

// { dg-module-cmi foo.bar:baz.bob }
