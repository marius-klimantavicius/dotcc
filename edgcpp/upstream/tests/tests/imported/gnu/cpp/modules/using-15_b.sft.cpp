//type: fp
//options:  --c++20 --modules
// PR c++/114868
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi M }

export module M;
export import :a;
