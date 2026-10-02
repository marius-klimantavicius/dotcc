//type:fn
//options_all:--c++11
//remark:[4.8] Missing diagnostic on invalid member overloading with ref-qualifiers
// 6/14/13  [EDGcpfe/14180]
//
// Missing diagnostic on invalid member overloading with ref-qualifiers
//
// In C++11, member functions can include ref-qualifiers (see entry of 4/23/13
// for EDGcpfe/8627).  The standard does not permit overloading two member
// functions with the same name and parameter types but with one having a
// ref-qualifier and the other not.  Previously, the front end incorrectly
// treated const and volatile qualifiers on the member function as part of the
// "parameter types", thereby failing to diagnose certain invalid cases.
//
// This is now fixed.
struct S {
  int f() const;
  void f() &;      // Previously silently accepted in C++11 mode.
};                 // Now an error.
