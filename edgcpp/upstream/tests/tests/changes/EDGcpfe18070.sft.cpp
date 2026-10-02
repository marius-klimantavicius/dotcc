//type:fp
//options_all:--c++11 --g++
//remark:[4.14] Spurious constexpr failure when adding/subtracting zero from null pointer
// 3/9/17   [EDGcpfe/18070]
//
// Spurious constexpr failure when adding/subtracting zero from null pointer
//
// The constexpr interpreter previously treated adding or subtracting zero from a
// null pointer as an evaluation failure, which in turn could lead to spurious
// errors.
//
// In C++11 mode, this is a regression introduced by version 4.13.  This is now
// fixed.
constexpr int* f() { return nullptr; }
constexpr int* g() { return f()+0; }
constexpr int* r = g();  // Previously an error.  Now okay.
