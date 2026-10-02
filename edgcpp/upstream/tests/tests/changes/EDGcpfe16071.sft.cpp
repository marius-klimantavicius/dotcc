//type:fp
//options_all:--microsoft
//remark:[4.10.1] Microsoft compatibility: Elision of invalid copy constructor
// 5/19/15  [EDGcpfe/16071]
//
// Microsoft compatibility: Elision of invalid copy constructor
//
// The resolution of EDGcpfe/15352 (see entry of 8/11/14) included a change to
// elide copy construction early in some Microsoft mode cases (when
// microsoft_version < 1900).  That change has now been extended to cover some
// additional cases.
//
// Previously, the instantiation of the default constructor of A<C> triggered an
// error, because the copy constructor of C cannot be fully generated (because
// it requires a call to the inaccessible copy constructor of B).  Now, the
// copy constructor of C is elided early, which prevents the error.
template<class T> struct A {
  A(): m(T()) {}  // Previously triggered an error.  Now okay.
  T m;
};
class B {
  B(B const &) {}
public:
  B() {}
};
struct C: B {};
A<C> a;
