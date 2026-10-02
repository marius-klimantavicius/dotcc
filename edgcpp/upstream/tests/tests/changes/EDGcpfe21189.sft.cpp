//type:fp
//options_all:--c++17 -A --exceptions -tused
//remark:[6.2] Fold-expressions in alias templates
// 12/2/20  [EDGcpfe/21189,EDGcpfe/21408,EDGcpfe/22012,EDGcpfe/23625]
//
// Fold-expressions in alias templates
//
// The front end previously incorrectly handled the substitution of fold
// expressions appearing in alias templates.
//
// That is now fixed.
template<bool> struct B { static constexpr bool value = true; };
template<bool> struct CX { typedef bool type; };
template<bool V> using C = typename CX<V>::type;
template <typename ...Ts> using And = B<(Ts::value && ...)>;
template<typename T> struct X {
  static const bool value = true;
};
template<typename ...Ts, typename = C<And<X<Ts>...>::value>>
void g(Ts &&...) {}
int main() {
  g(0, 0, 0, 0);  // Previously an error because the substitution in And
}                 // failed.
