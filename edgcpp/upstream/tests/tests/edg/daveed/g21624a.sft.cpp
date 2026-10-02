//remark:Narrowing and SFINAE
//options:--c++17;fp:--c++14;fp

template<int (*p)()>
constexpr bool is_constexpr(decltype(int{(p(), 0U)})) { return true; }
template<int (*p)()>
constexpr bool is_constexpr(...) { return false; }
 
constexpr int f() { return 0; }
int g() { return 0; }
 
int main()
{
  static_assert(is_constexpr<f>(0), "");
  static_assert(!is_constexpr<g>(0), "");
}
