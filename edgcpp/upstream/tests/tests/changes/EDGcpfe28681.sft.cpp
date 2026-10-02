//type:fp
//options_all:--gnu=150200
//remark:Assertion failure when casting 128-bit complex type
// 2/3/26   [EDGcpfe/28681]
//
// Assertion failure when casting 128-bit complex type
//
// An assertion failure (in lower_c99_complex_cast) could occur in certain
// configurations when casting from a 128-bit complex type.
// with --gnu_version=150200:
void f(__complex__ _Float128 v) {
  __complex__ long double x = v;
}
