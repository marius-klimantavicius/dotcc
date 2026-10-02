//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
module baz;

int Prod (int a, int b)
{
  return a * b;
}

int Square (int a, int b, int c)
{
  return Prod (Square (a, b), c);
}
