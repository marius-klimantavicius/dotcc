//type:fp
//remark:[4.10.1] Invalid handling of using-declaration referring to a base class member template
// 3/13/15  [EDGcpfe/16079]
//
// Invalid handling of using-declaration referring to a base class member template
//
// The front end sometimes incorrectly dealt with a using-declaration referring to
// a member template of a base class when it was followed by a member template
// declaration in the derived class that displaces the base-class import.  The
// problem, which usually manifested itself as a spurious ambiguity error, only
// occurred when the base and derived classes had different template
// parameterization levels.
//
// This is now fixed.
template<typename> struct P {};
struct B { template<typename T> void f(P<T>); };
template<int> struct D: B {
  using B::f;
  template<typename T> void f(P<T>);  // Should displace B::f.
};
int main() {
  D<0> d;
  P<int> x;
  d.f(x);  // Previously triggered an ambiguity error.  Now okay.
}
