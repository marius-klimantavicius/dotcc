//remark:CLang partial spec matching
//options:--c++14 --clang_v=70000;fn

           template <int c> class A {};
           template <long c> void f(A<c>) {}
           void g() { A<89> a;
             f(a);
           }
