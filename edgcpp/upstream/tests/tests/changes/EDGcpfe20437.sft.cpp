//type:fp
//options_all:--c++17
//remark:[6.0] Assertion failure in get_typeinfo_var
// 11/27/19 [EDGcpfe/20437,EDGcpfe/21093]
//
// Assertion failure in get_typeinfo_var
//
// In configurations that do lowering, an assertion failure (in get_typeinfo_var)
// had occurred when using typeinfo for some pointer-to-member functions in modes
// where exception specifications are considered part of a function type.
#include <typeinfo>
struct A {};
auto &x = typeid(void (A::*)() noexcept);
