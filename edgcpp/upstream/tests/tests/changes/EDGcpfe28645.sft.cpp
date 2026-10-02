//type:fp
//options_all:--c++20
// 9/1/26   [EDGcpfe/28645]
//
// Template template parameter expanding an empty enclosing pack
//
// During the instantiation of an enclosing template with an empty parameter pack,
// a template template parameter declared as a pack expansion of that enclosing
// pack was incorrectly treated as a nontype template parameter, which could
// trigger an internal error due to a failed assertion in
// update_template_param_symbol.  This regression was introduced by the changes
// for EDGcpfe/27707 (in version 6.8).
template<typename ... Ts>
void f() {
  auto l = []<template<Ts> typename ...>{ };
}
template void f<>();
