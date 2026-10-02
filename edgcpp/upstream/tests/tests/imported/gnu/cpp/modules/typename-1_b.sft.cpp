//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import foo;

struct X
{
  using type = int;
};

int main ()
{
  TPL<X> x {0,1};

  return 0;
}
