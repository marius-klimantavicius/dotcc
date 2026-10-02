//options_all:-r -x -tused
//options: --strict;cn:;rp

/*
Note that the reason for the "function like cast" interpretation is
that "::a" can *only* be used as a reference, and never used as a
declarator.  This fact is guaranteed by the syntax in the current
draft document.  Hence it "cannot be a declaration."
*/
extern float a;
extern float b;

extern float a;   // valid repeat declaration of a

main(){
        int (a)  ; // valid redeclaration of a
        int (::b); // valid function like cast of b
	return 0;
        }

