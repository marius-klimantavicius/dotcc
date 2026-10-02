//type:fn
//options_all:--c++17 -tused -A

int f(); // error, auto and decltype(auto) don't match
auto f() { return 42; } // return type is int
