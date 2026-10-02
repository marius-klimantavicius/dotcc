//type: fp
//options:  --c++20 --modules --c++20
// PR c++/116382
// { dg-additional-options "-fmodules-ts -std=c++20" }
// { dg-module-cmi m }

export module m;
import :a;
