//type:fp
//options_all:--c++20 -tused -A
//
consteval int f() { return 42; }
consteval auto g() { return f; }
consteval int h(int (*p)() = g()) { return p(); }
constexpr int r = h(); // OK

//cwg: 2129
//title: Non-object prvalues and constant expressions
//meeting: Jacksonville 2/16
//edg_status: Passes
