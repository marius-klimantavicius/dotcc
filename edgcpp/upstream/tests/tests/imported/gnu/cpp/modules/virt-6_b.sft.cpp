//type: fp
//options:  --c++20 --modules
// PR c++/115007
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi M }

export module M;
import :a;
