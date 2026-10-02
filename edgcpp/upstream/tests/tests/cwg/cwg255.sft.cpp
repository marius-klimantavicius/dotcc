//type:fp
//options_all:--c++20 -tused -A
    struct S {
        void operator delete(void*);
        void operator delete(void*, int);
    };
    void f(S* p) {
        delete p;    
    }

//cwg: 255
//title: Placement deallocation functions and lookup ambiguity
//meeting: Virtual 11/20*
//edg_status: Passes
