//type:fp
//options_all:--c --gnu_version 50400 --c99 --gcc
//remark:[6.0] GNU compatibility: Nonstandard folding
// 10/22/19 [EDGcpfe/21031,EDGcpfe/21904]
//
// GNU compatibility: Nonstandard folding
//
// The front end now emulates a few additional cases where GCC treats as constant
// an expression that is not constant according to the relevant standards.  First,
// comparing a variable with itself produces a constant true value with "==" or
// constant false value with "!=".
//
// Second, certain casts of null pointers are now treated as constants in GNU C
// mode.
int x;
struct S { int i: 1+(x == x); }; // Now accepted in GNU modes.
