//type:fp
//options_all:--c++20
//remark:[6.6] Pack expansion failure for concept used in requires clause
// 8/10/23  [EDGcpfe/26575]
//
// Pack expansion failure for concept used in requires clause
//
// In some fairly specific cases where a requires expression includes a parameter
// declaration clause and that requires expression is used in a concept
// constraining a function template, expansion of a requirement parameter pack
// could fail if the function template also contains an identically-named function
// parameter pack.  That regression (introduced in version 6.5 with the changes
// for EDGcpfe/25609,EDGcpfe/26117) is now fixed.
void f(int);
template<typename ... T>
concept C = requires(T ... t) { f(t ...); };
template<typename ... T> requires C<T ...>
int g(T ... t);
int i = g(1);  // Previously a spurious error.  Now okay.
