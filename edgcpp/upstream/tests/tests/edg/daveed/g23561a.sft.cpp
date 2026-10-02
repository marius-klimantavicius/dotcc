//remark:Clang __is_trivially_copyable oddity
//options:--c++17 --clang;fp:--c++17;fn

  struct E {};
  struct S { E const e; };
  static_assert(!__is_trivially_copyable(S));  // (1)
  struct SA { E const e[2]; };
  static_assert(!__is_trivially_copyable(SA));  // (2)
