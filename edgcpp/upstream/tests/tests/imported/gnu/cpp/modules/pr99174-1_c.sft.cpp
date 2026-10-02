//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import Foo;
import Foo;
import Bar;

int main ()
{
  Foo ();
  Bar ();
}
