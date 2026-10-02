//remark:SFINAE and static_cast
//options:--c++14;fp
  struct X { X(int, int = 1); };
  template<typename T> decltype(T{}, static_cast<X>(2)) f(T&&);
  auto r = f(3);
