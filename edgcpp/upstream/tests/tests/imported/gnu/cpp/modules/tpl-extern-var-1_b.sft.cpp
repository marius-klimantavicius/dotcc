//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import "tpl-extern-var-1_a.H";

int main ()
{
  Foo<int> ();
}
