//type:fp
//options_all:--c++20
//remark:[6.2] Overload resolution failure with conversions to arrays of unknown bound
// 2/2/21   [EDGcpfe/23816]
//
// Overload resolution failure with conversions to arrays of unknown bound
//
// Committee document P0388R4 allows for conversions of arrays of known bounds to
// pointers or references to arrays of unknown bounds.  This was implemented in
// EDGcpfe/21591 (in version 6.0), however, the implementation missed updating
// the check for an overloaded function match.  This caused spurious errors when
// attempting to call a function that had an overload set (where calling a
// function with no overload set would succeed).
//
// This is now fixed.
void f(int(&)[]);
void g(int(&)[]);
void g();
void test() {
  int arr[1];
  f(arr); // Accepted
  g(arr); // Previously rejected, now accepted
}
