//type:fp
//options_all:-w --microsoft_version 1910
//remark:[5.0] Defaulted move functions and exception specifications
// 2/16/18  [EDGcpfe/18959]
//
// Defaulted move functions and exception specifications
//
// Previously the generated move assignment operator of C called the move
// assignment operator of B<int>, which is implicitly deleted ("= delete").
// However, the C++ standard specifies that defaulted move assignment operators
// that are implicitly deleted should be ignored in overload resolution, which
// means the move assignment operator of C should call the copy assignment
// operator of B<int>.  This is now fixed in the front end, as is the similar
// issue with move constructors.
struct W { W& operator=(W const&); };
template<typename> struct B {
   B& operator=(B const&);
   B& operator=(B&&) noexcept(true) = default;
   W w;
};
struct C {
  B<int> m;
};
C g() {
  C c;
  c = g();  // Previously selected C's move assignment operator, triggering
}           // an error.  Now the copy assignment operator is called.
