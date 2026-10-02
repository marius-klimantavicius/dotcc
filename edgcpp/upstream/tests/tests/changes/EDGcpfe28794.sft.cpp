//type:fp
//options_all:--microsoft_version 1944 --target win64 --ms_c++20
//remark:Abort on __builtin_bit_cast in dependent context
// 4/13/26  [EDGcpfe/28794]
//
// Abort on __builtin_bit_cast in dependent context
//
// Applying the intrinsic __builtin_bit_cast (supported in some Clang and
// Microsoft modes) to a template-dependent operand could trigger an internal
// error in interpret.c.
//
// That is now fixed.
template<typename T> void g(T v) {
  constexpr long N = __builtin_bit_cast(long, v);
}
