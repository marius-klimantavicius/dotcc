//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import foo;

int main ()
{
  __is_constructible_impl<bool> x;
  return 0;
}
