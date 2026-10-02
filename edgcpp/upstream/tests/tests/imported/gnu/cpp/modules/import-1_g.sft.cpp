//type: fn
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
import Foop;

int main ()
{
  Thing ();
  Quux (1);  // { dg-error "not declared" }
  return 0;
}
