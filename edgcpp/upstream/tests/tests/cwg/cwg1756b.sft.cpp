//type:fn
//options_all:--c++17 -tused -A
struct S { explicit operator bool(); } s;

bool b2 = {s}; // #2
