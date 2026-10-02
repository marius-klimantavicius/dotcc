//type:fn
//options_all:--c++14
//remark:[6.8] Abort on recursive call of function with deduced return type
// 3/6/25   [EDGcpfe/27908]
//
// Abort on recursive call of function with deduced return type
//
// In certain cases where a function with a deduced return type is called
// recursively, the front end could abort with a failed assertion in
// finalize_deduced_return_type.
template<typename T>
auto g(T t) -> decltype(t(1L));
template<typename T>
auto f(T t) {
  g([] (auto a) { f(a); });  // Previously triggered an abort.  Now okay.
  return 1;
}
int i = f(1);
