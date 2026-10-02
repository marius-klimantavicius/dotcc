//type:fp
//options_all:--c++17
//remark:[5.0] Partial function template ordering and exception specifications
// 9/18/17  [EDGcpfe/18773]
//
// Partial function template ordering and exception specifications
//
// During overload resolution the front end sometimes failed to correctly
// distinguish candidate function templates based on their partial ordering if
// those function templates included template-dependent exception specifications.
// (The problem only occurred when exception specifications are part of the
// function type, as is now the case by default in C++17 mode).
//
// This is now fixed.
template<typename X, typename... Ts> struct NC {
  static constexpr bool val = __is_nothrow_constructible(X, Ts...);
};
template<typename X, typename... Ts>
  constexpr bool nc = NC<X, Ts...>::val;
template<typename T> int f(T&, T&) noexcept(nc<T, T>);
template<typename T> struct S {};
template<typename T> int f(S<T>&, S<T>&);
S<int> si;
int r = f(si, si);  // Previously an error in C++17 mode.  Now okay.
