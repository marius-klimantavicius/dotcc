//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

export module thing;
// { dg-module-cmi "thing" }

export int baz ();
