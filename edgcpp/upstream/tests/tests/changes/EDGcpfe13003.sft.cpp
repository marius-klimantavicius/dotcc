//type:fp
//options_all:--microsoft
//remark:[4.5] Microsoft compatibility: token pasting and inert macro names
// 7/2/12   [EDGcpfe/13003]
//
// Microsoft compatibility: token pasting and inert macro names
//
// In certain cases involving implicit token concatenation (when, according to
// the C++ Standard, token splicing via a ## operator evokes undefined
// behavior because the result is not a valid preprocessing token, but
// rescanning the expanded text in Microsoft mode implicitly forms an
// identifier), the front end failed to fully emulate the Microsoft
// preprocessor's behavior.  In particular, when the left operand of the ##
// operator is the closing parenthesis of a function-style macro that expands
// to the name of the current macro (which is marked as "inert," i.e., not to
// be expanded in order to prevent infinite recursion) and the result upon
// rescanning is the name of a different macro, the front end failed to expand
// the resulting macro invocation.  This is now fixed.
#define AZ i
#define B(x) x
#define A(x) B(A) ## x
int i = 0;
int j = A(Z);   // Previously expanded to AZ, now to i
