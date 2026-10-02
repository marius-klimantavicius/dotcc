//type:fp
//options_all:--c++11
//remark:[6.7] Constraints on C-style variadic function arguments and designated initializers
// 8/8/24   [EDGcpfe/17472,EDGcpfe/24212,EDGcpfe/25603,EDGcpfe/27479]
//
// Constraints on C-style variadic function arguments and designated initializers
//
// In C++11, C-style variadic arguments of class type must be trivially copyable,
// but the front end previously applied a more stringent constraint on such
// arguments.
//
// S here is trivially copyable, but the front end previously did not accept the
// example.  Now it does.  A similar relaxation applies to designated
// initializers.
#include <stdarg.h>
struct S { int x = 42; };
void g(char *fmt, ...) {
  va_list  ap;
  va_start(ap, fmt);
  S val = va_arg(ap, S);  // Previously an error.  Now accepted.
  va_end(ap);
}
