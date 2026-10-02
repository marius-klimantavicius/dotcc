//type:fn
//options_all:--c++17 -tused -A
enum E { a, b = true ? 1 : ((E)a, 0) };

//cwg: 1832
//title: Casting to incomplete enumeration
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
