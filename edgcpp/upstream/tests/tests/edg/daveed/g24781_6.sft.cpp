//remark:Explicit-this member functions
//options:--c++23;fp:--c++23 -DNEG;fn

struct C {
    C(C const&) = default;
    void operator+(this C);
    void operator+(this C, int);
#ifdef NEG
    void operator+(this C, int, int); // EXPECT: error: too many parameters for this operator function
#endif

    void operator-(this C);
    void operator-(this C, int);
#ifdef NEG
    void operator-(this C, int, int); // EXPECT: error: too many parameters for this operator function
#endif

    void operator*(this C);
    void operator*(this C, int);
#ifdef NEG
    void operator*(this C, int, int); // EXPECT: error: too many parameters for this operator function

    void operator[](this C); // EXPECT: error: too few parameters for this operator function
#endif
    void operator[](this C, int);
#ifdef NEG
    static void operator[](int); // EXPECT: error: operator may not be a static member function
#endif

#ifdef NEG
    void operator=(this C&); // EXPECT: error: too few parameters for this operator function
#endif
    void operator=(this C&, C const&);
#ifdef NEG
    void operator=(this C&, C const&, int); // EXPECT: error: too many parameters for this operator function
#endif

    C& operator=(this C&, C&&) = delete;
};

struct D {
    D& operator=(this D&, D const&) = default;
#ifdef NEG
    bool operator==(this D, D&) = default; // EXPECT: error
#endif
};

void test(C c) {
    +c;
    c + 2;

    -c;
    c - 2;

    *c;
    c * 2;

    c[2];
}
