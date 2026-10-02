//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import "leg-merge-7_a.H";
import "leg-merge-7_b.H";

int main ()
{
  return !foo<2> (2);
}
