//type:fp
//options_all:--gcc
//remark:[4.13] Internal error with macro definition of builtin function name
// 10/17/16 [EDGcpfe/17630,EDGcpfe/17635]
//
// Internal error with macro definition of builtin function name
//
// After the changes for the new builtin mechanism (Changes entry EDGcpfe/16431
// et al.), using a macro with the same name as a builtin function caused
// an internal error (assoc_source_line_modif: bad address).  Now fixed.
#define __builtin_vsnprintf __mingw_vsnprintf
void f() {
  const int __ret = __builtin_vsnprintf();
}
