//type:fp
//options_all:--c++11
//remark:[4.12] Folding bitwise-copied class members
// 9/12/16  [EDGcpfe/16900]
//
// constexpr copy construction from value-initialized members
//
// The front end previously reported a spurious "must have a constant value"
// error for cases in which bitwise copy initialization is required from a
// value-initialized class member in a context requiring a constant
// expression.  This is now fixed.
//
// 5/24/16  [EDGcpfe/16900]
//
// Folding bitwise-copied class members
//
// The front end previously failed to fold an invocation of a compiler-generated
// copy constructor for a class if one or more of the members involved a
// bitwise copy.  This is now fixed.
struct A {
    constexpr A() {}
    constexpr A(const A&) {}
};
struct B : A {
    constexpr B() : A(), m() { }
    int m;
};
constexpr B t1;
constexpr B t2(t1);  // Previously erroneously diagnosed as non-constant
                     // because of the bitwise copying of B::m
