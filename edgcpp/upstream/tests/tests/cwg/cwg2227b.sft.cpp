//type:fn
//options_all:--c++17 -w -tused -A

class A_ { ~A_(); };
struct B_ { A_ a = {}; };
auto *c = new B_ {};


