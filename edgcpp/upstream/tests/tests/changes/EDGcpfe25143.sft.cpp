//type:fp
//options_all:--c++17
//remark:[6.4] Spurious errors with generic lambda nested in variadic template
// 5/30/22  [EDGcpfe/25143]
//
// Spurious errors with generic lambda nested in variadic template
//
// The front end could produce spurious errors when a generic lambda using
// "auto" parameters (instead of, or in addition to, an explicit template
// parameter list) expands a parameter pack from a containing variadic
// template.
//
// The front end previously incorrectly reported on the line marked #1 that
// no instance of the lambda matched the argument types.  This is now fixed.
template<typename F, typename ... Args> struct Result {
  static F &fun;
  using X = decltype(fun(Args{}...));   // #1
  using type = int;
};

template<typename F, typename ... Args>
typename Result<F, Args...>::type g(F, Args...) {
  return 42;
}
template<typename ... Ps>
int v = g([](Ps..., auto)->int { return 42; }, 42, true);

int r = v<int>;
