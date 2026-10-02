//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import bob;

int frob (__sfinae_types::__two *p)
{
  return p->i;
}
