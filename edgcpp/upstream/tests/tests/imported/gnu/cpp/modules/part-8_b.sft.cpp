//type: fp
//options:  --c++20 --modules
// PR c++/110808
// { dg-additional-options "-fmodules-ts" }
// { dg-module-cmi group }

export module group;
export import :tres;
