//type:fp
//options_all:--c++17 -tused -A
struct S { explicit operator bool(); } s;
bool b1{s}; // #1

//cwg: 1756
//title: Direct-list-initialization of a non-class object (Resolved by 1467)
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
