//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules" }

import foo;

int main()
{
  C::i = 42;
}
