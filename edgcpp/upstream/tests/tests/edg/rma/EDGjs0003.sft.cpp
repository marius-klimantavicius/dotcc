//options_all:-r -x -tused
//options: --strict;cn:;cn

// Bug EDGjs00003
// This is a negative test.  The "(" after the close of struct A is
// really supposed to be there.

struct A {
        template <class T> A(T) {}
} ( 
A::A(int) {}



