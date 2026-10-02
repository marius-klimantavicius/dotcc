//type:fp
//remark:[6.1] Abort on attempt to substitute a call producing a complex object
// 5/8/20   [EDGcpfe/22704]
//
// Abort on attempt to substitute a call producing a complex object
//
// The front end sometimes aborted with an internal error in get_expr_rescan_info
// when substituting a call that produces its own enk_temp_init node to hold the
// result of the call.
//
// Here, the substitution of *T::r() resulted in an internal error.  That issue
// is now fixed.
template<typename T> struct M {
  template<typename F> auto f(F &&rf) -> decltype(rf(*T::r()));
};
template<typename E> struct A { E operator*(); };
struct C { void operator=(C&&); };
struct R { static A<C> r(); };
auto x = M<R>{}.f([](C &&p){ return 42; });
