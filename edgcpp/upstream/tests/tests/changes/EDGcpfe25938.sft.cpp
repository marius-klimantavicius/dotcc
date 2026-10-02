//type:fp
//options_all:--c23
//remark:[6.5] C23: Implement typeof and typeof_unqual operators
// 3/23/23  [EDGcpfe/25938,EDGcpfe/26054]
//
// C23: Implement typeof and typeof_unqual operators
//
// As described in WG14 papers N2927 and N2930, typeof can now be used to declare
// a type from either an existing type or an expression.  Similarly, typeof_unqual
// can be used to declare a type from either an existing type or an expression
// with type qualifiers removed.
//
// The --[no_]c23_typeof command-line option explicitly enables or disables this
// feature, regardless of the C version and dialect being emulated.
const int        a;
typeof(a)        b;  // equivalent to "const int b;"
typeof_unqual(a) c;  // equivalent to "int       c;"
