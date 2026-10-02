//type:fp
//options_all:--c++17 -tused -A
decltype(auto) x = new auto('a'); // allocated type is char, x is of type char

//cwg: 1851
//title: decltype(auto) in new-expressions
//meeting: Urbana-Champaign 11/14
//edg_status: Passes
