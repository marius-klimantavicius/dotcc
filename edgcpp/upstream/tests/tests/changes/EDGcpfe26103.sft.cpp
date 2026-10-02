//type:fp
//options_all:--c++20 -tused
//remark:[6.5] Spurious diagnostic on false requires-expression
// 2/28/23  [EDGcpfe/26103]
//
// Spurious diagnostic on false requires-expression
//
// Previously, an error was issued because the deduction for X(P<T>()) fails.
// However, such a failure in a requires-expression should only cause that
// expression to yield a false value and not trigger a diagnostic.  That is now
// fixed.
template<typename T> constexpr int f() noexcept { return 0; }
template<typename T> requires (f<T>() != 0) void g(T);
template<typename T> concept C = requires(T &t) { g(t); };
template<C> struct P {};
template<typename T> struct X {};
template<C T> X(T) -> X<int>;
template<typename T> auto h() {
  return requires { X(P<T>()); };  // Previously elicited an error.
}                                  // Now okay.
auto r = h<int>();
