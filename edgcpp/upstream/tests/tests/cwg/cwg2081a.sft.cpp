//type:fn
//options_all:--c++17 -tused -A

auto f() { return 42; } // return type is int
decltype(auto) f(); // error, auto and decltype(auto) don't match

//cwg: 2081
//title: Deduced return type in redeclaration or specialization of function template
//meeting: Jacksonville 2/18
//edg_status: Passes
