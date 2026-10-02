//options_all:-r -x -tused
//options: --strict;cn:;cn

class A { int a,b,c; A(); A(int); A(const A&,int i=0,int j=0); };
A a;         // error -- A() is inaccessible
A b = a;     // error -- A(A&) is inaccessible
A c = 1;     // error -- A(A&) is inaccessible

