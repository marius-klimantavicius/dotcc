//type:fp
//options_all:--c++20 -tused -A 
    struct S {
        void f(int);
    private:
        void f(double);
    };

    void g(S* sp) {
        sp->f(2);        
    }

//cwg: 600
//title: Does access control apply to members or to names?
//meeting: Virtual 11/20*
//edg_status: Passes
