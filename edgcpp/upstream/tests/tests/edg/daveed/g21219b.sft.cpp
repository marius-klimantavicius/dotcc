//remark:C++17 exception specs and explicit specialization
//options:--c++17;fp:--c++17 --clang;fp

  template<typename> struct X { static constexpr bool value = true; };
  template<typename T> void f(T&, T&) noexcept(X<T>::value);
  template<> void f(int& a1, int& a2) noexcept(X<int>::value) {}
