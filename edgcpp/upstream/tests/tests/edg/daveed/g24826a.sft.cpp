//remark:Substituted noexcept matching
//options:--c++17;fp:--c++17 --microsoft_v=1930;fp:--c++17 --clang_v=120000;fp:--c++17 --gnu=120000;fp

  bool const B = 0;
  template<int N>  void f(void (&f)() noexcept(N + B));
  template<> void f<0>(void (&)() noexcept(0+B));  // Previously a spurious
                                                   // error.  Now okay.

