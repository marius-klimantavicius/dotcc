//type: fp
//options: --c++20 --c++20 --modules
// PR c++/114683
// { dg-do compile { target c++20 } }
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi M }

export module M;
export import :a;
