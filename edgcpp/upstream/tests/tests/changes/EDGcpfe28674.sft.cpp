//type:fp
//options_all:--ms_c++20
//remark:Internal error in find_template_variable during constraint checking
// 6/1/26   [EDGcpfe/28674]
//
// Internal error in find_template_variable during constraint checking
//
// In Microsoft mode, the front end could abort with an internal error in
// find_template_variable while checking a constraint that referred to a variable
// template specialization, when that check was triggered in a further nested
// template context.
template<typename T> constexpr int v = 1;
template<typename T> struct X {
  template<typename V>
  operator V() requires(v<T> == 1);
};
auto b = [](auto p) {
  return requires { p + X<int>(); };  // Previously triggered an internal
}(1);                                 // error.  Now okay.
