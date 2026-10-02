//type:fp
//options_all:-tused -A --c++20
    struct outer {
        static int i;
        struct inner {
            void f() {
                struct local {
                    void g() {
                        i = 5;
                    }
                };
            }
        };
    };
