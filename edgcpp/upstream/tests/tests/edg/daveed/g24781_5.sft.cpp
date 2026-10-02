//remark:Explicit-this member functions
//options:--c++23;fp

struct X {
    void foo(this X const&);
    void foo(this X const&, int);

    void bar() const {
        foo();
    }
};
// :-)
