//type:fp
//remark:[6.3] Spurious substitution failure on cast
// 10/15/21 [EDGcpfe/24788]
//
// Spurious substitution failure on cast
//
// In some situations, where a constexpr dependent call is explicitly converted
// to a nondependent type, the front end could fail a substitution in GNU, Clang,
// and Microsoft C++ modes.
//
// Here the substitution of "bool(check_conv<To, From>())" was previously not
// correctly handled, causing the "conv" candidate to be erroneously discarded.
// That is now fixed.
template<bool> struct EnableIf {};
template<> struct EnableIf<true> {
  using Type = int;
};
template<typename To, typename From>
constexpr bool check_conv() {
  return sizeof(To) == sizeof(From);
}
template<typename To, typename From,
         typename EnableIf<bool(check_conv<To, From>())>::Type = 0>
To conv(From x) {
  union { From from; To to; } converter = { x };
  return converter.to;
}
long r = conv<long>(1.2);  // Previously an error.  Now okay.
