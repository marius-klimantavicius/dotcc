//remark:Explicit-this member functions
//options:--c++23;fp:--c++23 -DNEG;fn

struct C {
    void nonstatic_fun();
    
    int explicit_fun(this C c) {
#ifdef NEG
        nonstatic_fun();   // EXPECT: error
        auto x = this;     // EXPECT: error
#endif
        c.nonstatic_fun(); // ok
        static_fun();      // ok
        return 0;
    }
    
    static void static_fun() {
#ifdef NEG
        explicit_fun();     // EXPECT: error
        explicit_fun(C{});  // EXPECT: error
#endif
        C{}.explicit_fun(); // ok
    }
    
    void operator()(this C, char); // ok
};

C c;
int (*a)(C) = &C::explicit_fun; // ok

auto x = c.static_fun;     // ok
#ifdef NEG
auto y = c.explicit_fun;   // EXPECT: error
#endif
auto z = c.explicit_fun(); // ok

