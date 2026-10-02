//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
module Bar;

int frob (int a, float b)
{
  return bob (a) * bob (b);
}
