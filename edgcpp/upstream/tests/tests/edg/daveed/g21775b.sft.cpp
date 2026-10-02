//remark:Friend and constexpr in cp_gen_be
//options:--c++17 -tused -A;cp


  template<typename T> struct S {
    friend constexpr bool operator==(S, S) { return true; }
      // Previously, the "constexpr" specifier was not rendered by the
  };  // C++-generating back end.
  static_assert(S<int>() == S<int>());

