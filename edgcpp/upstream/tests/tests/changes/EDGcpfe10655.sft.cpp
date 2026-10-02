//type:fp
//options_all:--c --gcc
//remark:[4.2] GNU C compatibility: __builtin_pow and constant-expressions
// 4/30/10  [EDGcpfe/10655]
//
// GNU C compatibility: __builtin_pow and constant-expressions
//
// In GNU C modes with gnu_version >= 30400, the front end now accepts certain
// calls to __builtin_pow in constant-expression contexts.  Specifically, the
// call is folded to a constant if both arguments are constants and the second
// argument (the power) has a small nonnegative integer value.
//
// Calls to __builtin_powf and __builtin_powl are treated similarly.  The exact
// subset of calls that are folded by the front end differs somewhat from GCC's
// but calls that occur in constant-expressions in actual code are likely to be
// foldable by the front end.
int x[(int)__builtin_pow(7.1, 2.0)];  // Now accepted in GNU C mode because
                                      // 2.0 is a small integer value.
                                      // Same as "int x[50];".
