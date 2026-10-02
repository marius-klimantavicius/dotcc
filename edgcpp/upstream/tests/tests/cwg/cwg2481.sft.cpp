//type:fp
//options_all:--c++20 -A
    constexpr const int &r = 42;
    void f() {
     const_cast<int &>(r) = 23;  // Well-defined
     static_assert(r == 42);  // Ill-formed, non-constant with C++20 rules, we don't want this
   }

//cwg: 2481
//title: Cv-qualification of temporary to which a reference is bound
//meeting: Virtual 6/21
//edg_status: Passes
