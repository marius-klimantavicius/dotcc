//type:fp
//remark:[4.14] Spurious exception specification error on inheriting constructor with
// 9/1/17   [EDGcpfe/18541]
//
// Spurious exception specification error on inheriting constructor with
// default member initializer
//
// The front end previously issued a spurious error about a circular dependency
// when processing an inheriting constructor with an exception specification in
// a derived class with a default member initializer.
//
// This is now fixed.
struct B {
  B(int) noexcept;
};
struct D: B {
  using B::B;  // Previously produced a spurious error.  Now okay.
  int i = 42;
};
