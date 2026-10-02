//type: fp
//options:  --c++20 --modules -E
// { dg-do preprocess }
// { dg-additional-options "-fmodules-ts" }

#define baz [[]]
export module foo.bar baz;

int i;
