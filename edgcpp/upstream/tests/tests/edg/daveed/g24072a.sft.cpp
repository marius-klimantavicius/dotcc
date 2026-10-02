//remark:Default SDM initializer instantiation
//options:--c++17 --clang_v=110000;fp

  struct E { explicit E() = default; };
  template<typename T> struct S {
    static inline E value{};
  };
  E e = S<int>::value;

