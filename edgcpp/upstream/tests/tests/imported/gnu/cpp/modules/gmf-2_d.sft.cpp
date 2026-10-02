//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
import Foo;

// We see no MACRO

int main ()
{
  return !(MACRO (5) == -3);
}
