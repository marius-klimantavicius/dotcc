//type:fp
//options_all:--rtti
//remark:[4.10] Segfault when lowering typeid
// 5/13/14  [EDGcpfe/15150]
//
// Segfault when lowering typeid
//
// In cases where a variable is defined with the same mangled name as a type_info
// variable and that variable does not have the proper type, a segfault had
// occurred (in f_identical_types) when trying to lower a typeid operation.
//
// This example now results in a multiple-definition error in the linker.
#include <typeinfo>
extern "C" {
  int _ZTIi;    // _ZTIi is the typeinfo for "int" (in the IA-64 ABI)
}
void f(int x) {
  typeid(x).name();
}
