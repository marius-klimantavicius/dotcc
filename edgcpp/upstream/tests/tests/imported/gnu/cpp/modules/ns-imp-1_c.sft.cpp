//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

export import Foo;
export import Bar;

namespace Bob
{
void Widget ()
{
  Random ();
  Quux ();
}
}
