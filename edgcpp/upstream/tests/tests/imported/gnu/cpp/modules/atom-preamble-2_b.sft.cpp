//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
#if 1
export module bob;
// { dg-module-cmi bob }
#endif

import kevin;

X *f;

