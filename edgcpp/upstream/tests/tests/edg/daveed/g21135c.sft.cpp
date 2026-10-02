//remark:noexcept specifiers
//options:--c++17 -A -tused;fp

template<class T, class U>
struct A
{
   static constexpr bool const value = false;
};

template <class> struct S2
{
  void u(void) {}
  void v(void) noexcept(false) {}
};

int main(void)
{
  typedef decltype(&S2<int>::u) U;
  typedef decltype(&S2<int>::v) V;
  return A<U, V>::value;
}
