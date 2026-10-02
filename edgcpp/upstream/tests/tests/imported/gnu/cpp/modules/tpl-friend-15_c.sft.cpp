//type: fp
//options:  --c++20 --modules
// PR c++/114950
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi M }

export module M;
import :a;
import :b;
