//type:fp
//options_all:--microsoft
//remark:[4.7] Microsoft compatibility: concatenation with inert macro name
// 5/1/13   [EDGcpfe/13644]
//
// Microsoft compatibility: concatenation with inert macro name
//
// The Microsoft preprocessor allows concatenation of an identifier at the end
// of a macro expansion with another identifier that immediately follows the
// closing parenthesis of the macro invocation.  (Standard preprocessing
// maintains them as separate tokens.)  The front end failed to emulate this
// processing in Microsoft mode when the identifier following the macro
// invocation is an inert macro name, i.e., the name of a macro appearing in
// the expansion of that same macro; such macro names are internally marked to
// prevent recursive invocation of the macro, and that marking prevented the
// concatenation from occurring.  This is now fixed.
#define A(x) x
#define B A(x)B
int B;    // Now expands to "int xB;" in Microsoft mode
