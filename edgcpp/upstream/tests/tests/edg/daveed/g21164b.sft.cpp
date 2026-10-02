//remark:__reference_binds_to_temporary
//options:--clang_v=70000 -w;fp

  static_assert(!__reference_binds_to_temporary(int&, int&));
  static_assert(__reference_binds_to_temporary(int const&, long));
    // Now accepted in some Clang modes.

