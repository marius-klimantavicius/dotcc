//remark:Class constant template arguments
//options:--c++20;fp

  struct S {
    int f();  // Not const-qualified
  };
  template<auto V> int g() requires requires { V.f(); } = delete;
  template<auto V> int g();
  int r = g<S{}>();

