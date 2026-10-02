//type:fp
//options_all:--c++14
//remark:[4.14] Spurious SFINAE failure on nested member template reference
// 6/15/17  [EDGcpfe/18042,EDGcpfe/18482]
//
// Spurious SFINAE failure on nested member template reference
//
// In some fairly complex situations, the front end did not correctly substitute
// a reference to a member template in a function template signature, thereby
// triggering a spurious SFINAE failure (i.e., discarding a candidate template
// that should be treated as a viable candidate instead).
//
// Previously, this elicited a compiler error claiming no match for the call
// "v.e<1>()" due to a failure to correctly substitute "p.r.template e<N-1>()"
// in the declaration of E::g.  This problem is now fixed and this case is
// now accepted.
template<bool b> struct E;
template<typename ... Ts> struct V { V(); };
template <typename Head, typename ... Tail> struct V<Head, Tail...> {
  Head f; 
  V<Tail...> r;
  V(Head d,  Tail ...args) : f(d), r(args...) {}
  template<int N>
    auto e() -> decltype ( E<(bool)N>::template g<N>(*this) );
};
template<bool b> struct E {
  template<int N, typename ... Ts>
    static auto g(V<Ts...> &p) -> decltype(p.r.template e<N-1>());
};
template<> struct E<false> {
  template<int N, typename ... Ts>
    static auto g(V<Ts...> &a) -> decltype(a.f);
};
struct S {} s;
V<S*, S*> v(&s, &s);
static_assert(sizeof(v.e<1>()) == sizeof(&s), "");
