//type:fp
//options_all:--c++11
//remark:[4.10.1] Short-circuit evaluation in constant expressions
// 5/13/15  [EDGcpfe/15928]
//
// Short-circuit evaluation in constant expressions
//
// The front end previously issued a spurious "must have a constant value"
// error in C++11 mode if the unevaluated second operand of || or && appearing
// in a constant expression involves creation of a temporary with a
// non-trivial destructor.  This is now fixed.
struct C {
  ~C();
};
bool f(C);
void g() {
  static_assert(true || f(C()), "");  // previously a spurious error
}
