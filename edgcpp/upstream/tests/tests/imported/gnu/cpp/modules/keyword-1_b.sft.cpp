//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

import bob;

::import (d); // not import
import (e); // not import

void foo ()
{
  void *c = a;
  void *e = &b;
}

