//type:fp
//options_all:--c++20 -tused -A
    struct B {
        void f();
    };
    template<typename T> struct S: T {
        using B::f;
    };

//cwg: 852
//title: using-declarations and dependent base classes
//meeting: Virtual 11/20*
//edg_status: Passes
