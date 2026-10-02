//type:fp
//options_all:--c++20
//remark:Spurious user-defined literal lookup errors
// 12/16/25 [EDGcpfe/28589]
//
// Spurious user-defined literal lookup errors
//
// Previously, in some fairly complex cases involving fold expressions in template
// constraints, lookup for user-defined literals could spuriously fail.  For
// example, with --c++20:
template<bool ... Bs> requires (Bs && ...)
int f();
namespace ns {
  int operator ""_udl(unsigned long long);
}
using ns::operator ""_udl;
int i = f<true, true>() + 2_udl;
