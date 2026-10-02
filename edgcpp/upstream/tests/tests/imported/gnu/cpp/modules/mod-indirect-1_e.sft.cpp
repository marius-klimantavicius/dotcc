//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
import Bar;

int main ()
{
  return frob (2, 4) != 4 * 6;
}
