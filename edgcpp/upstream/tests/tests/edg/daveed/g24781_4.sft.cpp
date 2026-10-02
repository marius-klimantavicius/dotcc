//remark:Explicit-this member functions
//options:--c++23;fp

struct C {
    operator int(this C const&);
    int test() const {
        return *this;
    }
};

int test(C const& c) {
    return c;
}
