//options_all:-r -x -tused
//options: --strict;cp

extern int ivalue(int);
// 811p13 - ambiguity resolution favors declaration
// (in ambiguity between fn decl with redundant parens around arg
// vs object decl with fn-style cast as initializer)
int f(int (a)) { return ivalue(a); }  // function declaration

