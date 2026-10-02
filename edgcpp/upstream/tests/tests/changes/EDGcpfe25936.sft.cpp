//type:fp
//options_all:--ms_c++17 --no_rtti
//remark:[6.5] Microsoft C++ compatibility: support parsing typeid when no_rtti is set
// 4/26/23  [EDGcpfe/25936]
//
// Microsoft C++ compatibility: support parsing typeid when no_rtti is set
//
// In recent MSVC releases, typeid is parsed even when MSVC's runtime type
// information support is disabled (via passing /GR-).  To match this behavior,
// the above code is now accepted in Microsoft mode when --no_rtti is passed as a
// command-line option.
#include <typeinfo>

void foo() {
  auto& ti1 = typeid(int);
}
