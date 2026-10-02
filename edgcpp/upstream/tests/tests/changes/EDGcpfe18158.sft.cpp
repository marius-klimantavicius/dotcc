//type:fp
//remark:[4.14] Unbounded loop in constexpr interpreter on default member initializer
// 4/5/17   [EDGcpfe/18158]
//
// Unbounded loop in constexpr interpreter on default member initializer
//
// In some cases where a default member initializer depended on the address of a
// data member, the front end could end up in an unbounded loop in the constexpr
// interpreter (in function translate_interpreter_offset).
//
// This is now fixed.
struct B {};
struct D: B {} ;
struct X { B *x; };
struct L {
  X const *p;
  constexpr L(X const &x): p(&x) {}
};
struct S {
  D d;
  L l{X{&d}};  // Previously triggered an unbounded loop.
};
