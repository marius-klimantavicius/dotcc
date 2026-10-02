//type: fp
//options:  --c++20 --modules --c++20
// { dg-additional-options "-fmodules-ts -std=c++2a" }

import "legacy-7_a.H";

#ifdef throw
#error barf
#endif

