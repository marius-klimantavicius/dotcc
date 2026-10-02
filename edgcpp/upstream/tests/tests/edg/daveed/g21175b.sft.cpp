//remark:Nontype template arguments and local static variables
//options:--c++17;fp

  template <int*> struct S {};
  auto f() {
    static int x = 0;
    return S<&x>{};
  }
