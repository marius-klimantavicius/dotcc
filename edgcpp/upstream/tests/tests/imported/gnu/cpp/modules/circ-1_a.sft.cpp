//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
export module Bob;
// { dg-module-cmi Bob }

export int bob ();
