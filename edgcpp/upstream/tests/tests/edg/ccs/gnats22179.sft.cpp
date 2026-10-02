//type:fn
//options::--gnu_version 80100
//options_all:--c++20

template <typename T>
int func() noexcept(T(0));

template <typename T>
struct A {
  using type = decltype(func<T>());
};

A<A<int>>::type foo;
