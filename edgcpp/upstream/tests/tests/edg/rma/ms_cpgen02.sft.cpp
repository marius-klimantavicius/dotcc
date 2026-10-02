//options_all:-r -x -tused --microsoft
//options:;cp:;ln

#if TEST_NUMBER==2
#define INST 1
#endif

struct S {
    S();
    template <class U> S(const U&) { }
    template<> explicit S(S*const&p) { }
};

int main() {
#if defined INST
        S x;
        S *const &rcp = &x;
        S y(rcp);
#endif
}

