//remark:friend constexpr definitions
//options:--c++20;fp

template<typename T> struct S {
  friend constexpr int f(S<T>) { return sizeof(T); }
};

template<typename T> struct X {
  double x[f(T{})];
};

X<S<int>> isi;


