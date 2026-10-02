//type:fp
//options_all:--c++17 -tused -A
template <class T, class U> struct my_is_same {
  const bool value = false;
};
template <class T> struct my_is_same<T,T> {
  const bool value = true;
};
  int i;
  int&& f();
  auto           x2a(i);    // decltype(x2a) is int
  decltype(auto) x2d(i);    // decltype(x2d) is int
  auto           x3a = i;   // decltype(x3a) is int
  decltype(auto) x3d = i;   // decltype(x3d) is int
  static_assert(my_is_same<int, decltype(x2a)>().value);
  static_assert(my_is_same<int, decltype(x2d)>().value);
  static_assert(my_is_same<int, decltype(x3a)>().value);
  static_assert(my_is_same<int, decltype(x3d)>().value);

//cwg: 1958
//title: decltype(auto) with parenthesized initializer
//meeting: Lenexa 5/15
//edg_status: Passes
