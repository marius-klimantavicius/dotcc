//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
import nsdmi;

int main ()
{
  Bob b;

  return b.m != 42;
}
