//remark:Rescanning subexpression of ek_integral_constant kind
//options:--c++17;fp

  template<typename T> T &&declval() noexcept;
  struct W { bool value; };
  template<typename T> constexpr W wrap = { noexcept(*declval<T>()) };
  template<typename T> void f(T &&v) noexcept(wrap<T>.value) {}
  void g(int *x) { f(x); }
