//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import Foo;

int main ()
{
  return Check (0) ? 0 : 1;
}
