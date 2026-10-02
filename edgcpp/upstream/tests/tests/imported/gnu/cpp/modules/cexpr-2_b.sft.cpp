//type: fp
//options:  --c++20 --modules
// { dg-additional-options "-fmodules-ts" }
import sqrt;

static_assert (sqrt(81) == 9, "waaa!");
