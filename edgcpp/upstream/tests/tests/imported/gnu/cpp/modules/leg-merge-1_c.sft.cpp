//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import "leg-merge-1_a.H";
import "leg-merge-1_b.H";

int main ()
{
  return bob (0);
}
