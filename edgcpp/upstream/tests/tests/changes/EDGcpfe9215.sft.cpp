//type:fp
//options_all:--c --gcc --c99
//remark:[4.0] GNU C/C99 compatibility: Implicit int
// 11/4/08  [EDGcpfe/9215, EDGcpfe/9333]
//
// GNU C/C99 compatibility: Implicit int
//
// In C89 mode, a function definition can omit its return type, in which case
// type int is assumed.  Other declarations (e.g., variable declarations and
// function declarations that aren't definitions) can also omit "int", provided
// another specifier (e.g., static) is present.  In C99 mode, there is no such
// "implicit int" rule by default.
//
// In GNU C and C99 modes, the implicit int rule now applies to all function,
// variable, and typedef declarations.  A warning is issued for any declaration
// relying on this rule (except when declaring "main").
f() { return 3; }  // Now accepted (with a warning) in GNU C99 mode.
x;                 // Now accepted in GNU C and C99 modes (with a warning).
