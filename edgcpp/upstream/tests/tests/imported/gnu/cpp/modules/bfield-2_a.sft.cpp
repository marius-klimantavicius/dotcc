//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module foo;
// { dg-module-cmi foo }

export struct ting
{
  int a;
  int b : 3;
  int c : 5;
};
