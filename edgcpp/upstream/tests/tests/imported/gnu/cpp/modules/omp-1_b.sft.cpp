//type: s
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts -fopenmp" }
// { dg-require-effective-target pthread }

import foo;

unsigned ary[64];

int main ()
{
  frob (ary);
}
