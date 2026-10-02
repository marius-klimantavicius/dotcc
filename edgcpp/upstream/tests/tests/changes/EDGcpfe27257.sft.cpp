//type:fp
//options_all:--c++17 --gnu_version=110400
//remark:GCC compatibility: Severity of narrowing in brace-notation cast
// 1/7/26   [EDGcpfe/27257]
//
// GCC compatibility: Severity of narrowing in brace-notation cast
//
// Technically, this is invalid in C++11 because the initialization of arr[1]
// with make_int() requires a narrowing conversion.  However, GCC only issues a
// warning in this case and the front end now emulates that (GCC also only issues
// a warning in various other cases that ought to be errors, but the front end
// already emulated those other cases).
struct S { unsigned arr[2]; };
int make_int();
S g() {
  return S{ 1, make_int() };  // Normally a narrowing error.
}                             // Now a warning in GNU C++ mode.
