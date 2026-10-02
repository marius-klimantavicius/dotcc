//type: fp
//options:  --c++20 --modules
// { dg-additional-options {-fmodules-ts -fno-module-lazy} }

import  "pr99283-5_b.H";

static_assert(!__traits<unsigned>::__min);
