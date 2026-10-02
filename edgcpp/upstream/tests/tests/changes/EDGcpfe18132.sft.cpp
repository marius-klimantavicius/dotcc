//type:fp
//remark:[4.14] Spurious error on some calls to constexpr functions in templates
// 3/19/17  [EDGcpfe/18132]
//
// Spurious error on some calls to constexpr functions in templates
//
// In some relatively complex situations, the front end produced a spurious
// "expression must have a constant value" calls appearing in templates.
//
// Previously, an error was issued for the static_assert condition in this
// example.  Now it is accepted.
template<int... Ns> struct C;
template<int N> struct C<N> { static constexpr int V = N; };
template<int N, int... R> struct C<N, R...> { static constexpr int V = N; };
template<typename T> struct X {
  constexpr operator int() { return sizeof(T); }
};
struct S {
  template<typename... Ts> void f() {
    static_assert(C<(int)X<Ts>()...>::value != 0, "");
  }
};
