//type:fn
//options_all:--c++20 -tused
    struct S {
        static void g(int*) {}
        static void g(long) {}
    } s;

    void foo() {
        (&s.g)(0L);
    }
