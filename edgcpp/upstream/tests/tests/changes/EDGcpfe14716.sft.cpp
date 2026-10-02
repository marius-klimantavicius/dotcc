//type:fn
//options_all:--microsoft
//remark:[4.9] Microsoft compatibility: Instantiating templates to find candidates
// 12/5/13  [EDGcpfe/14716]
//
// Microsoft compatibility: Instantiating templates to find candidates
//
// In C++, template classes must sometimes be completed so that candidates for a
// function call can be made available (friend declarations and member operator
// declarations in particular).  Microsoft compilers, however, do not always do
// this.
//
// Here, resolving the call to g in the decltype expression should normally
// trigger the instantiation of P<void> (the type of make<T>()) to allow
// argument-dependent lookup (ADL) to find a possible better candidate declared
// as a friend in P<void>.  That instantiation would trigger an error because
// P<void>::i is declared with an incomplete type I.  Previously, in Microsoft
// modes with microsoft_version >= 1600, that error was also issued, but now this
// case is accepted in such modes.  (The Microsoft conditions for avoiding the
// instantiation are unclear.  The front end avoids it only in decltype arguments
// during partial function template instantiations, which matches all the cases
// we have encountered thus far.)
struct I;
template<class> struct P { I i; };
template<class T> T& make();
template<class T> bool g(T const&);
template<typename T> struct C {
  template<class U> static decltype(g(make<T>())) f();
  static const bool value = sizeof(f<T>());
};
template struct C<P<void>>;
