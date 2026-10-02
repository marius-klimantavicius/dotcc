//remark:Substitution of decltype
//options:--c++17;fp

  template<typename> void f();
  template<typename T> using X = decltype((f<T>));
  template<typename T> struct S {};
  template<typename T> S<X<T>> g();
  auto r = g<int>();  // Previously triggered an error.
