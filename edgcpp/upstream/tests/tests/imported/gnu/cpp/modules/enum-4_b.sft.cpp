//type: fp
//options:  --c++20 --modules
// { dg-additional-options -fmodules-ts }

import bob;

static_assert (same<int, float>::value == 0);
static_assert (same<int *, int *>::value == 1);
