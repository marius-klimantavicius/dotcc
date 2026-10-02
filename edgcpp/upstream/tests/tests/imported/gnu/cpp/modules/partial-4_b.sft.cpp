//type: fp
//options:  --c++20 --modules --c++20
// PR c++/114947
// { dg-additional-options "-fmodules-ts -std=c++20" }
// { dg-module-cmi M }
export module M;
import :part;
