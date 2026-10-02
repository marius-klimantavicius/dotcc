//type:fn
//options_all:--c++20 -tused
  template <typename Ty>
  struct B {
    Ty n;
    consteval B() = default;
  };

  B<int> b;
