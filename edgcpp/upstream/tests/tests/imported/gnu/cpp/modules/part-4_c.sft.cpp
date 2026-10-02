//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts -fdump-lang-module" }

import foo;

void f ()
{
  auto f = foo ();

  decltype (f)::inner x;
}
