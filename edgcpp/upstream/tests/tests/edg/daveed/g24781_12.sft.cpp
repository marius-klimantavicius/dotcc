//remark:Explicit-this member functions
//options:--c++23;fp

struct B {
    constexpr int f() && { return 0; }
};

struct D : B {
    using B::f;
    constexpr int f(this B&&) { return 1; }
};

static_assert(D().f() == 1);
