//type:fp
//options_all:--gcc --gnu_version 40803
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
_Static_assert((int*)(void*)0 == (int*)0, "");  // Accepted in GNU C mode.
