//type:fn
//options:--c++17;fp:--c++20
//options_all:-W -r --diag_suppress 174 --set_flag=no_checking_pragmas

int x[12];
constexpr int two = 2;
constexpr int f() { return 42; }
int r = x[f(), two]; // deprecated
int s = x[(f(), two)]; // OK
int t = x[f(), 2]; // deprecated
int u = x[(f(), 2)]; // OK
