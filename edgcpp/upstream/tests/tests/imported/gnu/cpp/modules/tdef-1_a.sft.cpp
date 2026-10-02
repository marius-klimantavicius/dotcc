//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

export module tdef;
// { dg-module-cmi tdef }

export struct A
{
  typedef int I;
};
