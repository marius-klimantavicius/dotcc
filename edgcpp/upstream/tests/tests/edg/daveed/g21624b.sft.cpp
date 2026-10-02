//remark:Narrowing and SFINAE
//options:--c++17;fp

template<int (*p)()>
  constexpr bool narrows(decltype(int{(p(), 0U)})) { return true; }
template<int (*p)()>
  constexpr bool narrows(...) { return false; }
int g() { return 0; }
static_assert(!narrows<g>(0));
