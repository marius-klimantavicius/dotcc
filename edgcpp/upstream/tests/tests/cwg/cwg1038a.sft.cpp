//type:fn
//options_all:--c++20 -tused
    struct S {
        static void f(int);
        static void f(double);
    };
    S s;
    void (*pf)(int) = &s.f;

//cwg: 1038
//title: Overload resolution of &x.static_func
//meeting: Kona 11/23
//edg_status: Passes
