//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

export module frob;
// { dg-module-cmi frob }

namespace
{
  int nope;
}

export int f (int)
{
  return nope;
}

int g (int *a);
