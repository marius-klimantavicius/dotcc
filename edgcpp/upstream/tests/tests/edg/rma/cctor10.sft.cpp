//options_all:-r -x -tused
//options: --strict;cn

class A { A(A); };                    // Error
class B { B(B,int); };                // Okay
class C { C(C,int=0); };              // Error
class D { D(D&); };                   // Okay
class E { E(E&,E); };                 // Okay
class F { F(F&,F=0); };               // Error
class G { G(G&,int=0,G=0); };         // Error

