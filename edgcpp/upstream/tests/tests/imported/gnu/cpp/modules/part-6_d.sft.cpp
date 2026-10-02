//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }
module foo;
import :bits;

Foo *Foo::Factory ()
{
  return new Foo ();
}
