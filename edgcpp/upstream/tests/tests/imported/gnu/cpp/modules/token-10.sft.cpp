//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

#define semi ;
export module foo.bar:baz.bob semi

// { dg-module-cmi foo.bar:baz.bob }
