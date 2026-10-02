//type:fp
//options_all:--c++17 -A --exceptions -tused -w
//remark:[6.0] Incorrect substitution when combining alias templates
// 11/21/19 [EDGcpfe/21254,EDGcpfe/21630,EDGcpfe/21957,EDGcpfe/21985,
//           EDGcpfe/22031,EDGcpfe/22041]
//
// Incorrect substitution when combining alias templates
//
// Version 5.1 of the front end introduced a regression in the substitution of
// alias templates when multiple alias templates were combined in complex ways.
//
// Previously, the substitution of CondInt<WrapRet<F>> with F = Callable produced
// an incorrect type independent of Callable.  As a result, the call was treated
// as ambiguous.  That is now fixed.
template<typename T> T make();
template<typename> struct Wrap;
template<typename> struct Func;
template<typename R, typename... Ps> struct Func<R(Ps...)> {
  template<typename F> using RetType = decltype(make<F>()(make<Ps>()...));
  template<typename F> using WrapRet = Wrap<RetType<F>>;
  template<typename C> using CondInt = int;
  template<typename F, typename = CondInt<WrapRet<F>>>
    Func(F);
};
template<int> struct X {};
void g(Func<void(X<0>)>);
void g(Func<void(X<1>)>);
struct Callable { void operator()(X<0>); } callable;
int main() {
  g(callable);  // Previously considered ambiguous.  Now okay.
}
