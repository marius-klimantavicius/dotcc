//type:fn
//options_all:--c --microsoft
//remark:[4.4] Microsoft C mode block-extern function declarations
// 4/18/11  [EDGcpfe/11476]
//
// Microsoft C mode block-extern function declarations
//
// Previously, in Microsoft C mode, a block-extern function declaration followed
// by a file-scope declaration of the same function always resulted in the
// creation of two distinct a_routine entries.
//
// This allowed the front end to accept incompatibilities in those declarations
// (they were silently accepted).  Now, the front end only creates the second
// entry if the declarations are actually incompatible: In the example above only
// one entry is created.  Furthermore, to be accepted, two incompatible
// declarations must have "interchangeable" return types and a warning is issued.
void f() { extern int g(), h(); } 
long g() { return 1; }  // Now a warning; a second routine entry is created.
void h() {}             // Now an error.
