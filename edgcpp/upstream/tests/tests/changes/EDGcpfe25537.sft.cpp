//type:fp
//options_all:--c++20
//remark:[6.4] Abort on deduction involving class-type nontype template parameter
// 10/6/22  [EDGcpfe/25537,EDGcpfe/25660]
//
// Abort on deduction involving class-type nontype template parameter
//
// This previously aborted due to an attempt at emitting a spurious diagnostic
// without an available source position.  The following example:
//
// also aborted with an attempt to dereference a null expression stack pointer.
// Those problems are now fixed.
template<int> struct Str {};
template<int> void f() {}
template<Str> void f() {}
void g() { f<0>(); }
