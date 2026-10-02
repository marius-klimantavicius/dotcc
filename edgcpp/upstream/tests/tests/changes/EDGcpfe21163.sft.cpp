//type:fp
//options_all:--g++
//remark:[6.2] Infinite loop with typedefs and attributes
// 11/30/20 [EDGcpfe/21163]
//
// Infinite loop with typedefs and attributes
//
// It had been possible to get the front end into an infinite loop when adding
// certain attributes to function types using typedefs.  That is now fixed.
typedef void f1_t(int*, int*, int*) __attribute__((nonnull(1)));
typedef f1_t f2_t __attribute__((nonnull(2)));
f2_t fn2 __attribute__((nonnull(3)));
