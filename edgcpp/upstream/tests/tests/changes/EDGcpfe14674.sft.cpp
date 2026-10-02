//type:fp
//remark:[4.9] Spurious error on uninitialized subobject in GNU SFINAE context
// 11/18/13 [EDGcpfe/14674]
//
// Spurious error on uninitialized subobject in GNU SFINAE context
//
// In GNU C++11 mode the front end sometimes issued an error in SFINAE contexts
// (where errors are normally not emitted) for expressions requiring the default-
// initialization of a class with a const member or a reference member.
//
// Previously, the tentative deduction of (1) for the call to Test<S>::test
// triggered an error in GNU C++11 mode, when instead that candidate should just
// silently be ignored and (2) be selected instead.  This is now fixed.
struct S { int const N; };
template<typename T> struct Test {
  template<unsigned long> struct Val2Type {};
  typedef char YesType;
  typedef char (&NoType)[2];
  template<typename U> static YesType test(Val2Type<sizeof(new U)>*); // (1)
  template<typename U> static NoType test(...);                       // (2)
  static bool const value = (sizeof(test<T>(0)) == sizeof(YesType));
};
int r = Test<S>::value;
