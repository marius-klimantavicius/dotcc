//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

// tu1
export module A;
// { dg-module-cmi A }

export import :Foo;
export int baz();
