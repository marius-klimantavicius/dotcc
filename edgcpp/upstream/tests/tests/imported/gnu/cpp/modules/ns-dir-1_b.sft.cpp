//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import bob;

void foo ()
{
  elsewhere::frob ();
}
