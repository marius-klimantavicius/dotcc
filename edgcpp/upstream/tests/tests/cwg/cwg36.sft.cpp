//type:fp
//options_all:--c++20 -tused -A
    namespace A {
            int i;
    }
    
    void f()
    {
            using A::i;
            using A::i;             // error: double declaration
    }

//cwg: 36
//title: using-declarations in multiple-declaration contexts
//meeting: Virtual 11/20*
//edg_status: EDGcpfe/23847
