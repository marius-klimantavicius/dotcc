//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module module1;
// { dg-module-cmi module1 }

export import :submodule1;
