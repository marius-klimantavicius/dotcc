//type:fp
//options_all:--clang
//remark:[4.11] GNU/Clang compatibility: Current instantiation treated as template-dependent
// 7/10/15  [EDGcpfe/16312]
//
// GNU/Clang compatibility: Current instantiation treated as template-dependent
//
// While parsing class template members in their generic form, the enclosing class
// is called the "current instantiation" in the C++ standard.  The types of
// expressions referring to members of the current instantiation are not always
// considered "template dependent".  Now, however, they are in Clang and GNU
// modes.
//
// Here the type of s corresponds to the current instantiation, and therefore s.N
// is known to be int and thus not dependent.  As a result, g in g(s.N) should be
// looked up only when the template is first parsed, and since no g is visible
// the case is in error (the front end issues an error in strict mode).  In GNU
// and Clang modes, however, the front end now treats the type of s as an unknown
// template-dependent type (and hence also the type of s.N), and so the code is
// accepted assuming that looking up g will succeed if the template is eventually
// instantiated with real arguments.
template<typename> struct S {
  int N;
  void f() {
    S s;
    g(s.N);  // Now accepted in GNU and Clang modes.
  }
};
