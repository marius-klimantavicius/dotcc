//type:fp
//options_all:--c++14
//remark:[6.4] Spurious error for default argument with nested template argument lists
// 9/16/22  [EDGcpfe/25547]
//
// Spurious error for default argument with nested template argument lists
//
// When ">>" is used at the end of a default argument to terminate two template
// argument lists, a spurious error was issued.
template<typename T> struct C { };
template<typename T> int v = 0;
void f(int i = v<C<int>>);  // Previously a spurious error.  Now okay.
