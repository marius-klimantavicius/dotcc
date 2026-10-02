//type:fp
//options_all:--c++17
//remark:[5.1] Incorrect handling of bit fields in constexpr interpreter
// 5/29/19  [EDGcpfe/21172]
//
// Incorrect handling of bit fields in constexpr interpreter
//
// Unsigned bit fields were incorrectly treated as signed in the constexpr
// interpreter, leading to spurious errors.
//
// Furthermore, the operators &=, |=, and ^= could produce results that were not
// limited to the width of a bit field to which they were applied.  E.g.:
//
// That is now fixed.
struct S { unsigned bf:4; };
static_assert([]{ S s{}; s.bf = 15; return s.bf; }() == 15);
     // Previously failed.  Now okay.
