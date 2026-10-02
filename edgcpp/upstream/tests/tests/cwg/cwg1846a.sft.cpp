//type:fn
//options_all:--c++17 -tused -A
struct S {
constexpr S() = default; // ill-formed: implicit S() is not constexpr
};

//cwg: 1846
//title: Declaring explicitly-defaulted implicitly-deleted functions
//meeting: Urbana-Champaign 11/14
//edg_status: EDGcpfe/22080
