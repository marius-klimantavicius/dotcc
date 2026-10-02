//type:fp
//options_all:--c++14
//remark:[5.0] Premature removal of unneeded instantiations leads to assertion failure
// 1/4/18   [EDGcpfe/19040]
//
// Premature removal of unneeded instantiations leads to assertion failure
//
// During wrap up processing the front end removes unneeded instantiations, but
// that processing had occurred a little too early, resulting in an assertion
// failure (in set_parent_scope) in some cases.
template<typename F> int f(F &&);
auto l = [](auto) {
  return [](auto){};
};
decltype(f(l(0))) x;
