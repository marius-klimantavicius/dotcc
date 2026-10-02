//type:fp
//options_all:--microsoft
//remark:[4.12] Microsoft compatibility: Instantiating templates to find candidates
// 7/22/16  [EDGcpfe/17189]
//
// Microsoft compatibility: Instantiating templates to find candidates
//
// The changes for EDGcpfe/14716 cause the front end to avoid instantiating
// class templates in cases where such an instantiation might be needed to find
// a function or operator candidate.  It appears MSVC no longer exhibits that
// nonstandard behavior and the front end now no longer emulates it when
// microsoft_version >= 1900.
//
// Previously, in Microsoft C++ modes, X<S<int>> did not match the partial
// specialization (2) because the call to f with T = A<int> in the decltype
// construct did not force the instantiation of A<int> and therefore the friend
// declaration (3) was not found.  Instead, X<S<int>> was considered an instance
// of the primary template (1), triggering an incomplete type error for the
// definition of x.  Now, that behavior is limited to microsoft_version < 1900;
// when microsoft_version >= 1900, the example is accepted.
template<typename T> T val();
template<typename, typename = void> struct X;                 // (1)
template<typename T> struct X<T, decltype(f(val<T&>()))> {};  // (2)
template<typename> struct S {
  friend void f(S&) {}                                        // (3) 
};
X<S<int>> x;
