//remark:Copy elision and Microsoft mode
//options:--c++20;fn:--c++20 --microsoft_v=1944;fp

    struct S {
      constexpr S(int *p): p(p) {}
      constexpr S(S const&): p(nullptr) {}
      constexpr ~S() {
        if (p != nullptr) *p = 1;
      }
      int *p;
    };
    constexpr S g(int n) {
      return true ? S(&n) : S(&n);  // (1)
    }
    static_assert(g(42).p == nullptr);  // Normally not constant.  Now okay in
                                        // Microsoft mode.

