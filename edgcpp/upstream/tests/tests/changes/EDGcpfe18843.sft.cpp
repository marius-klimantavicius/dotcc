//type:fp
//options_all:--microsoft_v 1900 -tused -w
//remark:[5.0] Microsoft bugs: typename behavior emulation in member templates
// 10/9/17  [EDGcpfe/18843]
//
// Microsoft bugs: typename behavior emulation in member templates
//
// The changes for EDGcpfe/8401 caused the front end to emulate nonstandard
// behavior of the Microsoft compiler (in Microsoft bugs mode) with respect to the
// semantics of "typename".  However, in some cases involving member templates,
// they also caused the front end to reject valid code that the Microsoft compiler
// accepts.
//
// Previously, in Microsoft bugs mode, this case triggered a warning on the
// "typename" keyword and the call to f() in the definition of d() resulted in an
// error.  That is now fixed: The case is now accepted.
template<typename> struct S {
  S();
};
template<typename T> struct X {
  using P = S<const T>;
  template<typename U = T>
    static auto f() -> decltype(U::g(typename U::P())) {
      return 0;
    }
  virtual int d() const { return f(); }
};
struct Z: X<Z> {
  static int g(S<const Z> const&);
};
X<Z> xz;
