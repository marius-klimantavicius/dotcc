//type:fp
//options_all:--microsoft
//remark:[4.11] Microsoft compatibility: token concatenation across macro invocation boundary
// 7/24/15  [EDGcpfe/15843]
//
// Microsoft compatibility: token concatenation across macro invocation boundary
//
// The Microsoft preprocessor allows concatenation of the last token of a
// macro expansion with the token following the macro invocation to produce
// a single token.  The front end previously preserved the token break between
// a macro expansion and the following text but has now been changed to allow
// the concatenation in Microsoft mode.
#define AB 1,2
#define C(x) x
#define D(x) x
#define E D(D(A) ## B)
int a[2] = C({E});   // Previously produced "{A B}", now "{1,2}"
