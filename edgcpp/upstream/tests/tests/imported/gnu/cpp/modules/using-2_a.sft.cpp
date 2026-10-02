//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export module bob;
// { dg-module-cmi bob }

namespace N {
export int foo (int a)
{
  return -a;
}
int bar (int a)
{
  return -a;
}
}

export using N::foo;
using N::bar;

