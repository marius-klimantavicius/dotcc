//type:fp
//options_all:--c++17
//remark:[5.0] Explicit instantiation and noexcept specifiers
// 10/10/17 [EDGcpfe/18787]
//
// Explicit instantiation and noexcept specifiers
//
// In C++17 mode, noexcept specifiers are part of function types.  However, they
// need not be specified in explicit instantiation directives.  The front end,
// however, did not permit them to be omitted in such cases.
//
// That is now fixed.
template<typename T> void f() noexcept(true) {}
template void f<int>();  // Previously an error in C++17 mode.  Now okay.
