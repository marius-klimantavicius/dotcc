//remark:__is_signed treatment in Clang mode
//options:--c++11 --clang;fp

  template<typename> struct E {
    enum { e = 1 };
  };
  template<typename T> struct S {
    static_assert(E<T>::e, "");
    static const bool __is_signed = (T)(-1) < 0;  // Previously an error in
  };                                              // Clang mode.  Now okay.

