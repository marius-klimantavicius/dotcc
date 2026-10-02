//type:fp
//options_all:--gnu_version=80000 --no_exceptions --c++17
//remark:[5.1] Exception specifications as part of function types of function parameters when
// 7/2/19   [EDGcpfe/21463]
//
// Exception specifications as part of function types of function parameters when
// exceptions disabled
//
// The change for EDGcpfe/19495 made it so that exceptions continue to make up
// part of the function type in C++17 mode, even when --no_exceptions is
// specified.  However, the front end was still not taking this into account when
// determining whether two function types had compatible exception specifications.
// This could lead to spurious errors.
//
// This is now fixed.
void f(void (*g)()) {}
void f(void (*g)() noexcept) {} // Spurious redeclaration of "f" error
