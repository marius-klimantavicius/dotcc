//type: fp
//options:  --c++20 --modules --c++20
// PR c++/113405
// { dg-additional-options "-fmodules-ts" }
// { dg-require-effective-target c++20 }

import M;

struct test {};
using quux = corge_alias<test>;
