//type: fp
//options:  --c++20 --modules
// PR c++/116364
// { dg-additional-options "-fmodules-ts" }

export module foo;
export import :part;
