//type: fp
//options: 
# 0 "./ext/interface4.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./ext/interface4.C"
# 9 "./ext/interface4.C"
# 1 "./ext/interface4.h" 1
#pragma interface
namespace N {
        typedef int A;
}
inline void g ( ) {
        static N :: A a = 0;
        a = a;
}
# 10 "./ext/interface4.C" 2

void f ( ) {
        g ( );
}
