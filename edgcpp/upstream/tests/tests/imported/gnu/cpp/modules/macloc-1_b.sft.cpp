//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }

export module edith;
// { dg-module-cmi edith }

#define STUART(X) X

import STUART(agnes);

export void STUART(gru) ();
