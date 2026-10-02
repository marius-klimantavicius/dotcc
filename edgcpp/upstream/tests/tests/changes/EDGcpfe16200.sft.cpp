//type:fp
//options_all:--microsoft
//remark:[4.11] Microsoft compatibility: recursive macro invocation
// 7/28/15  [EDGcpfe/16200]
//
// Microsoft compatibility: recursive macro invocation
//
// According to the C and C++ Standards, a macro name appearing in its own
// expansion is not considered for further expansion.  The Microsoft
// preprocessor, however, relaxes this rule, expanding such nested invocations
// as long as they do not produce expanded text that is similar to that of the
// earlier invocation, which could lead to unbounded recursion.  The front end
// previously followed the standard rules for suppressing recursive macro
// invocations, but it has now been changed in Microsoft mode to use the more
// relaxed approach.
#define invoke(M, arg) M arg
#define X(arg) invoke(Y, (arg))
#define Y(arg) arg
#define Z invoke(X, (0))
int i = Z;   // Previously expanded Z to "invoke(Y, (0))", now to "0"
