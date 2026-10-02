//type:fn
//options_all:--c++26
auto&& f1() {
    return 42;  // ill-formed
}
const double& f2() {
    static int x = 42;
    return x;   // ill-formed
}
auto&& id(auto&& r) {
    return static_cast<decltype(r)&&>(r);
}
auto&& f3() {
    return id(42);  // OK, but probably a bug
