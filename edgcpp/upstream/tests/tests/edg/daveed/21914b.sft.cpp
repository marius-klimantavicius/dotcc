//remark:Implicit initializers
//options:--c++14;fp

struct S {
    int i;
    struct N { N() {} } n;
};
struct X {
    S s;
    double d = 1.0;
} x = { 42 };
