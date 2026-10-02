//type:fp
//options_all:--ms_c++17
//remark:[6.5] Spurious error on deduced initializer_list cast
// 10/31/22 [EDGcpfe/25757]
//
// Spurious error on deduced initializer_list cast
//
// This previously elicited an error during the deduction of the template argument
// for std::initializer_list (complaining that an expression must have a constant
// value).  That is now fixed.  (This was a regression introduced in version 6.3
// of the front end by the changes for EDGcpfe/25537,EDGcpfe/25660.)
#include <initializer_list>
struct S { ~S(); };
void g(S x) {
  std::initializer_list{ x };  // Previously an error.  Now okay.
}
