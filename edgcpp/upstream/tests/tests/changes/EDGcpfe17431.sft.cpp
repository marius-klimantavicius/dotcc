//type:fp
//remark:[4.12] Spurious error on constexpr constructor with variant member initializer
// 7/25/16  [EDGcpfe/17431]
//
// Spurious error on constexpr constructor with variant member initializer
//
// In modes accepting constexpr constructors, the front end issued a spurious
// error claiming an anonymous union is left uninitialized when a constexpr
// constructor initializes a variant member that is not the first data member
// of its enclosing anonymous union.
//
// This is now fixed.
struct S {
  union {
    float first;
    short second;
  };
  constexpr S(): second(0) {}  // Previously triggered an error.  Now okay.
};
