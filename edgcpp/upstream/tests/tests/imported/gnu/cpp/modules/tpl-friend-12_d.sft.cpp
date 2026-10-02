//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi M }

export module M;
export import :B;
export import :C;

export int go_in_module();
