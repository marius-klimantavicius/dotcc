//remark:noexcept-specifier visibility
//options:--c++20;fn

  template<typename> concept C = true;
  void f(C auto) noexcept (f(42)) {}
    // Previously erroneously accepted.  Now an error.
