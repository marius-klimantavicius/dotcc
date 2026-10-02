//type:fp
//options_all:--clang
//remark:[5.1] GNU/Clang compatibility: spurious error on variable with alignment
// 7/8/19   [EDGcpfe/21490]
//
// GNU/Clang compatibility: spurious error on variable with alignment
//
// An error had been issued when a variable declaration with an alignment
// attribute had occurred after a definition of that variable without an alignment
// attribute.  That error is now suppressed in GNU emulation mode and a warning is
// given in Clang emulation mode.
float f = 0.0;
extern float f __attribute__((aligned(8)));
