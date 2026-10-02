//options_all:-r -x -tused
//options: --microsoft -n;cp

template <class T> struct S {
    S();
    template <class U> S(const U&) { }
    template<> explicit S(S*const&p) { }
};
S<int> x;

