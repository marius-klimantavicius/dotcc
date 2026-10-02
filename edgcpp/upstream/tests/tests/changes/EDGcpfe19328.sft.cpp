//type:fn
//remark:[5.0] Mutable members and constant expressions
// 2/20/18  [EDGcpfe/19328]
//
// Mutable members and constant expressions
//
// The front end previously ignored the "mutable" specifier when loading a data
// member from a constant in the evaluation of a constant expression, thereby
// accepting invalid constant expressions.
//
// This is now fixed.
struct S { mutable int x; };
constexpr S s{42};
constexpr int r = s.x;  // Previously accepted.  Now an error.
