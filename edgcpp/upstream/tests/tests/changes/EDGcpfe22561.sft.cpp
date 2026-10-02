//type:fp
//options_all:--g++ --c++17
//remark:[6.1] Constant-evaluation of __builtin_memcmp
// 3/31/20  [EDGcpfe/22561]
//
// Constant-evaluation of __builtin_memcmp
//
// The interpreter previously did not correctly evaluate calls to __builtin_memcmp
// with operands that point to non-byte-array objects.
//
// That is now fixed.  The interpreter can only fold __builtin_memcmp calls for
// operands pointing to constant objects of integral or array-of-integral types
// (that is approximately the same constraint as GCC and Clang impose).
int const x = 1;
int const y = 1;
static_assert(__builtin_memcmp(&x, &y, sizeof(x)) == 0);
  // Previously failed.  Now okay.
