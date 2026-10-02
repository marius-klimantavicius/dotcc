//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts -g" }

import frob;

struct thongy : thingy
{
  void X ()
  {
    thongy w;
  }
};
