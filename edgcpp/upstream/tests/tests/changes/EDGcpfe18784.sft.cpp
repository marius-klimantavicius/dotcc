//type:fp
//options_all:--gnu_version 60000
//remark:[6.2] Constant-evaluation of unsigned integer to same-sized signed integer
// 10/8/20  [EDGcpfe/18784,EDGcpfe/18847,EDGcpfe/20144,EDGcpfe/21386,
//           EDGcpfe/23124,EDGcpfe/23447]
//
// Constant-evaluation of unsigned integer to same-sized signed integer
//
// The front end previously did not correctly handle the constant-evaluation of a
// conversion from an unsigned integer that has its most significant bit "on" to
// an unsigned type of the same size.
//
// That is now fixed.
constexpr unsigned f(unsigned p) {
  return (int)p + 1;  // The cast was previously not handled correctly.
}
static_assert(f(~0) == 0, "Unexpected");  // Previously failed.  Now okay.
