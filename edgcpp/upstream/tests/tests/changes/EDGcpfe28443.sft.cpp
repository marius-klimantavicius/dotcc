//type:fp
//options_all:--clang
//remark:[6.8] Constexpr ctor-initializers for bases containing only zero-length arrays
// 9/19/25  [EDGcpfe/28443]
//
// Constexpr ctor-initializers for bases containing only zero-length arrays
//
// Previously, this triggered an error about a constexpr constructor having to
// initialize its direct base classes.  However, base classes with no data are
// not so constrained, and the front end now recognizes the case above as such:
// No error is issued for this case anymore.
struct E { int e[0]; };
struct D: E {
  constexpr D() {}  // Previously an error.  Now okay.
};
