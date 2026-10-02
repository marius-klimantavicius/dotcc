//type:fp
//options_all:--g++ --c++11
//remark:[5.0] GNU compatibility:  __builtin_offsetof
// 1/26/18  [EDGcpfe/19156]
//
// GNU compatibility:  __builtin_offsetof
//
// Previously, in all GNU C++ modes, the front end disallowed uses of
// __builtin_offsetof that do not produce a constant.  Now, such uses are
// permitted when gnu_version >= 40600.  Furthermore, the constexpr interpreter
// has been updated to fold the corresponding bok_offset entries.
struct S { char buf[1]; };
constexpr unsigned g(unsigned n) {
  return (unsigned)__builtin_offsetof(S, buf[n]);  // Now accepted in some
}                                                  // GNU C++ modes.
static_assert(g(10) == 10, "");  // Okay in some GNU C++ mode.
