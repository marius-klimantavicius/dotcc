//type:fp
//remark:[4.2] Visibility of variable with parenthesized initializer
// 2/19/10  [EDGcpfe/10456]
//
// Visibility of variable with parenthesized initializer
//
// In some GNU and Microsoft modes, a variable declaration is invisible in its
// own parenthesized initializer (see Changes entries of 12/12/02 and 7/27/01).
// However, in those modes the front end previously also made prior declarations
// of the same entity invisible, which does not match the behavior of the GNU
// and Microsoft compilers.
//
// This is now fixed.
extern int x;
int x((int)&x);  // Previously x in "&x" was not found in some GNU and
                 // Microsoft modes.
