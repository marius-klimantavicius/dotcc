//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
import Frob;

int main ()
{
  return twice (2) != 4;
}
