//type:fp
//options_all:--c++03 --gnu=90100
//remark:[6.0] Assertion failure in builtin_function_type
// 10/2/19  [EDGcpfe/21869]
//
// Assertion failure in builtin_function_type
//
// In GNU emulation mode when gnu_version >= 90000 or clang emulation mode when
// clang_version >= 90000, the use of __builtin_is_constant_evaluated in pre-C++11
// modes would result in an assertion failure in builtin_function_type.
// That is now fixed.
void f() {
    __builtin_is_constant_evaluated();
}
