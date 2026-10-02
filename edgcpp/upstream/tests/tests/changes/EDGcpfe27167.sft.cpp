//type:fp
//options_all:--c++11
//remark:[6.7] Explicit function template specialization could lead to undefined behavior
// 4/15/24  [EDGcpfe/27167]
//
// Explicit function template specialization could lead to undefined behavior
//
// Missing skip_typerefs had caused undefined behavior when explicitly
// specializing a function template that was originally declared using an alias
// template.
template<typename T> using fn_t = T(T);
template<typename T> fn_t<T> f;
template<>
int f(int) {
  return 0;
}
auto v = f(1);
