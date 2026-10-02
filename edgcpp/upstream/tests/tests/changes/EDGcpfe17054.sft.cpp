//type:fp
//options_all:--c++11
//remark:[4.12] Constant comparison of addresses for equality
// 7/29/16  [EDGcpfe/17054]
//
// Constant comparison of addresses for equality
//
// The front end previously reported a spurious error when constant addresses
// are compared for equality in a context requiring a constant expression.
// This is now fixed.
int i1 = 1;
int i2 = 2;
constexpr int *pi1 = &i1;
constexpr int *pi2 = &i2;
static_assert(&i1 != &i2, "");   // Previously an error, now okay
static_assert(pi1 != pi2, "");   // Previously an error, now okay
