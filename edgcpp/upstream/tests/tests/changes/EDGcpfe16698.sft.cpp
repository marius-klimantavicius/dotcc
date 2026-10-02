//type:fp
//options_all:--c++11
//remark:[4.11] Calling a constexpr function through a reference
// 12/4/15  [EDGcpfe/16698,EDGcpfe/14417]
//
// Calling a constexpr function through a reference
//
// The front end previously incorrectly rejected a call to a constexpr
// function via a constexpr reference to that function in a context requiring
// a constant expression.  This is now fixed.
constexpr int foo() {
  return 11;
}
constexpr int (& r)() = foo;
constexpr int i = r();   // Previously rejected as non-constant
