//type:fp
//options_all:--c++17 -A -tused -w
struct A_ { };
int a_() {
        A_ * b = new A_;
        using A2 = A_;
        b->A_::~A2();
        using C = int;
        0 .C::~C();
        return 1;
}

//cwg: 2292
//title: simple-template-id is ambiguous between class-name and type-name
//meeting: San Diego 11/18
//edg_status: Passes
