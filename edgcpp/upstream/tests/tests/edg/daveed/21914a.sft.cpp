//remark:Implicit initializers
//options:--c++14;fp


struct M {
    int z;
    bool b;
    struct N {
        int i;
        N() : i(102) {}
        operator int() { return i; }
    } n;
};
struct S {
    M m;
    int i = 1;
} s = { 100, true };
