//remark:__is_same_as
//options:--gnu=70300 --c++14;fp:--gnu=60300 --c++14;fn:--clang --c++11;fp

#ifdef __clang__
  static_assert(__is_same(int*, int*), "Test 1 failed");
    // Now accepted in Clang C++ mode.
#else
  static_assert(!__is_same_as(int, int*), "Test 2 failed");
    // Now accepted in GNU C++ mode.
#endif
