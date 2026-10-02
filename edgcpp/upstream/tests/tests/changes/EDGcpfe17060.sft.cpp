//type:fp
//remark:[4.12] SFINAE and nontype template argument conversions
// 6/7/16   [EDGcpfe/17060]
//
// SFINAE and nontype template argument conversions
//
// The front end previously failed to trigger a SFINAE failure on certain
// seemingly innocuous conversions on nontype template arguments.  This could
// lead to spurious errors later on.
//
// In this example, candidate (1) is considered with T = D.  The argument &T::f
// becomes &D::f which much be matched to type int (D::*)().  In many contexts,
// &D::f can be resolved to the first member of B, producing a value of type
// int (B::*)(), but when matching a nontype template parameter, that is not a
// valid conversion and hence (1) must be discarded.  Previously, that discarding
// did not happen and candidate (1) was selected, causing spurious errors later
// on.  That is now fixed.
struct B {
  int f();
  void f() const;
};
struct D: B {};
template<typename T, T> struct V {};
template<typename T> decltype(V<int (T::*)(), &T::f>(), 42) g(T);  // (1)
template<typename T> char g(...);
static_assert(sizeof(g<D>(D{})) == 1, "Unexpected!");
  // Previously triggered a spurious error about an incompatible type
  // for the second template argument of V.
