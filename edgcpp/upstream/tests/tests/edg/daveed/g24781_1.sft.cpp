//remark:Explicit-this member functions
//options:--c++23;fp:--c++23 -DNEG;fn

struct A {
    void f(this A, int);
    static void f(A);

    void h() {
        f(0);
    }
};

struct B {
    void f(this B const&);
    static void g(int);

    static void h() {
        B{}.f();
#ifdef NEG
        f(B{});    // EXPECT: error
        (f)(B{});  // EXPECT: error
        (+f)(B{}); // EXPECT: error
#endif
    }

    void m(int);
    void n() {
#ifdef NEG
        (+m)(0);  // EXPECT: error
#endif
    }
};

void test(B b) {
#ifdef NEG
    auto f = b.f; // EXPECT: error
#endif
}

struct C {
    static void f(C);
    void f(this C, int);

    void h() {
        f(0);
    }
};
