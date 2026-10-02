//type:fp
//options_all:--c++17
//remark:[6.2] Assertion failure "missing typeinfo variable"
// 11/30/20 [EDGcpfe/23168]
//
// Assertion failure "missing typeinfo variable"
//
// Generating typeinfo information for a function type with an exception
// specification had, in some cases, resulted in an assertion failure
// ("missing typeinfo variable").  Now fixed.
#include <typeinfo>
void f() {}
typedef void (*fp)(void) noexcept(true);
auto *g() { return (fp)f; }
const std::type_info &x = typeid((true, g)());
