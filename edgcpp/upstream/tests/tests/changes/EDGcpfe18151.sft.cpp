//type:fp
//options_all:--c++11
//remark:[4.14] Spurious "limited lifetime" folding error
// 4/6/17   [EDGcpfe/18151]
//
// Spurious "limited lifetime" folding error
//
// The front end sometimes issued a spurious error claiming a subexpression
// is not a constant due to a reference addressing a temporary with a limited
// lifetime.
//
// This is now fixed.
struct S {};
struct X {
  constexpr X(S const&) {}
};
constexpr X x{{}};  // Previously, triggered a spurious error.
