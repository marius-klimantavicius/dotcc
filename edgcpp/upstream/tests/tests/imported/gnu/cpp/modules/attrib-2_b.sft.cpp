//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules -Wmaybe-uninitialized" }

import M;

int main()
{
  A a;
  f(a);
}
