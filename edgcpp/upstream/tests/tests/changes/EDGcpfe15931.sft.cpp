//type:fp
//options_all:--c++11
//remark:[4.10.1] Abort on foldable C++11-mode logical operator with destructible operand
// 1/23/15  [EDGcpfe/15931]
//
// Abort on foldable C++11-mode logical operator with destructible operand
//
// The changes for EDGcpfe/14139 introduced a regression in C++11 mode that
// manifested itself in some cases involving a logical operator (&& or ||) that
// is foldable because its first operand is a constant allowing the short-
// circuiting of the second operand's evaluation, and that second operand would
// otherwise have required a destruction.  The IL in some of those cases was
// invalid (still recording the potential destruction), causing the IL lowering
// process to abort later on.
//
// This is now fixed.
struct A {
  ~A();
  bool m;
};
struct B {
  B(bool);
  ~B();
};
template<class T> int operator&&(bool, T);
void f(B) {
  f(false && A().m);  // Previously produced wrong IL and an abort
}                     // during IL lowering.
