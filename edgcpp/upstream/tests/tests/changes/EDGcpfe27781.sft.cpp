//type:fp
//options_all:--gn 110000 -tused
//remark:[6.7] Spurious error on __builtin_bit_cast with class template instance
// 12/5/24  [EDGcpfe/27781]
//
// Spurious error on __builtin_bit_cast with class template instance
//
// The front end previously failed to instantiate the destination type
// of a __builtin_bit_cast construct when needed.  This resulted in
// spurious errors about the destination type's size not matching the
// operand's size.
//
// This is now fixed.
template<typename T> struct S {
  char val[sizeof(T)];
};
auto r = __builtin_bit_cast(S<int>, 42);  // Previously an error.
                                          // Now okay.
