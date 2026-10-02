//type:fp
//remark:[4.13] Defaulted special members and constexpr
// 1/31/17  [EDGcpfe/17802]
//
// Defaulted special members and constexpr
//
// Defaulted special members may be implicitly constexpr.  However, the front end
// sometimes failed to recognize this, leading to spurious errors.
//
// This is now fixed.
struct B {
  constexpr B() {}
  constexpr B(B const&) {}
};
struct D: B {
  constexpr D(): B() {}
  D(D const&) = default;  // Should be implicitly constexpr.
};
constexpr D d1, d2(d1);   // Previously an error.  Now okay.
