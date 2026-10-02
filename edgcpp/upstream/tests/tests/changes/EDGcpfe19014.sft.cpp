//type:fp
//options_all:--c++14
//remark:[5.0] Spurious errors on unevaluated non-constant-expression operators in C++14 mode
// 12/7/17  [EDGcpfe/19014]
//
// Spurious errors on unevaluated non-constant-expression operators in C++14 mode
//
// C++14 relaxed the rules for constant-expressions compared to C++11.  However,
// the front end still imposed some C++11 constraints in C++14 mode.
//
// In C++11, the expression "B ? 42 : throw" is never a constant-expression
// because of the "throw" expression, but in C++14 (and later) it is a constant-
// expression when B is true.  The front end previously spuriously issued a
// diagnostic in C++14 mode.  That is now fixed.
template<bool B, double (&RA)[B ? 42 : throw]> struct S;
    // Previously an error in C++14 mode.  Now okay.
