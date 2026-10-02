//type:fp
//options_all:--ms_c++20 --microsoft_v 1949
//remark:[6.8] Microsoft compatibility: Return value copy elision in constant evaluation
// 10/3/25  [EDGcpfe/28473]
//
// Microsoft compatibility: Return value copy elision in constant evaluation
//
// The return statement in (1) normally does not involve a copy constructor
// invocation (it must be elided in C++17).  MSVC, however, does introduce a
// copy constructor invocation during the constant evaluation and that causes
// the embedded pointer of the result to be set to null, thus making the
// static_assert condition true.  The front end now emulates that behavior.
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
