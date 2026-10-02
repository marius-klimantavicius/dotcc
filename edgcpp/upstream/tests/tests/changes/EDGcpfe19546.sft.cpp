//type:fp
//options_all:--c++14
//remark:[5.0] Out-of-bounds numeric parts of user-defined literals appearing in templates
// 4/11/18  [EDGcpfe/19546]
//
// Out-of-bounds numeric parts of user-defined literals appearing in templates
//
// The front end previously reported spurious errors for user-defined literals
// appearing in templates when the numeric part of the literal overflowed or
// underflowed the parameter type for an ordinary literal operator, even when
// a raw literal operator or literal operator template is visible.  This is
// now fixed.
template<char...> int operator ""_xyz() { return 0; }
template<typename T> int f() {
  return 0x1'0000'0000'0000'0000_xyz;  // Too large for 64-bit unsigned long
                                       // long, previously an error
}
int i = f<int>();
