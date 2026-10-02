//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

#define bob(n) fred
export module foo.bar:bob;

// { dg-module-cmi foo.bar:bob }
