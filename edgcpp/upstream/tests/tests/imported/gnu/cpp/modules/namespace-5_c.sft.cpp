//type: fp
//options:  --c++20 --modules
// PR c++/100707
// { dg-additional-options "-fmodules-ts" }

import A.B;
namespace A::B {}
