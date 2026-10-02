//remark:Default constructibility and unions
//options:--c++17;fp

  template<typename T> struct DefaultConstructible {
    template<typename U, typename = decltype(U())> static char test(int);
    template<typename> static int test(...);
    static constexpr bool value = sizeof(test<T>(0)) == sizeof(char);
  };
  struct S {
    union {
      const int n;
    } u;
  };
  static_assert(!DefaultConstructible<S>::value, "Error");
                                          // Previously failed.  Now okay.
