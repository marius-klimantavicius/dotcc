//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import foo;

struct B
{
  aint m;
};

static_assert (alignof (B) == 16);
