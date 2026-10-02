//type:fp
//options_all:--c++20 -tused -A
    int j;    // global namespace
    struct S {
        void f() {
            struct local2 {
                void g() {
                    j = 5;
                }
            };
        }
    };

//cwg: 191
//title: Name lookup does not handle complex nesting
//meeting: Virtual 11/20*
//edg_status: Passes
