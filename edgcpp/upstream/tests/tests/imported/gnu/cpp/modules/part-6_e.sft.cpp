//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import foo;

int main ()
{
  Foo::Factory ()->Func ();
}
