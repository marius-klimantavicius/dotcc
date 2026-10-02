//remark:User-defined conversions and copy/move construction
//options:--c++11;fn

struct X {
  template <class T>
  operator T();

  template <class T, class = void>
  explicit operator T();
};

struct S {
  S(S const&);
  S(S&&);
};

  S s(X{});
