//type:fp
//options_all:--microsoft_version 1910
//remark:[4.14] Spurious Microsoft-mode error on call to constexpr template member function
// 4/6/17   [EDGcpfe/18089,EDGcpfe/18142]
//
// Spurious Microsoft-mode error on call to constexpr template member function
//
// In Microsoft mode, a call to a constexpr member function instance from within
// the same parent class sometimes failed to fold because the instance was not
// full instantiated.  In constant-expression context, this resulted in spurious
// errors.
//
// This is now fixed.
template<typename T> struct C {
  template<typename U> static constexpr int f(U*) { return 1; }
  template<typename> static constexpr int f(...) { return 0; }
  static constexpr int v = f<T>(nullptr);
};
int t = C<int>::v;  // Previously a spurious error in Microsoft mode.
