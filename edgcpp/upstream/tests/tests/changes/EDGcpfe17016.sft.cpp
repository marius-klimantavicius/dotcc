//type:fn
//remark:[4.11] Template substitution of sizeof/alignof
// 4/13/16  [EDGcpfe/17016]
//
// Template substitution of sizeof/alignof
//
// The front end previously failed to treat as a failure (error or SFINAE) certain
// substitutions of sizeof with an operand of incomplete type or function type.
//
// This is now fixed (as is the similar problem with alignof).
template<typename U, int N = sizeof(U)> int g();
struct I;
int r = g<I>();  // Now an error.  Previously erroneously accepted with
                 // N == 0.
