//type:fn
//remark:[5.1] Defaulted copy constructors and rvalue reference members
// 9/6/18   [EDGcpfe/19910]
//
// Defaulted copy constructors and rvalue reference members
//
// An rvalue reference member in a class type causes any associated defaulted
// copy constructor to be deleted.  E.g.:
//
// However, the front end previously applied that rule unreliably, particularly
// in cases with user-declared move constructors or move assignment operators.
//
// That is now fixed.
struct X {
  int &&r;
  X(X const&) = default;
  X(X&&);
};
void f(X &p) {
  X x(p);  // Previously erroneously accepted.  Now an error.
}
