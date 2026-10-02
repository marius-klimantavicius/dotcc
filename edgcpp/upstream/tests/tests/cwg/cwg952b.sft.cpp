//type:fn
//options_all:--c++20 -tused -A
    struct A {
        typedef int I;    // public
    };
    struct B: private A { };
    struct C: B {
        void f() {
            I i1;         // error: access violation
        }
        I i2;             // error 
        struct D {
            I i3;         // error
            void g() {
                I i4;     // error
            }
        };
    };
