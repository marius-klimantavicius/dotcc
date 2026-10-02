//remark:Explicit-this member functions
//options:--c++23;fp

constexpr int copy_capture(int i) {
    auto lambda = [=](this auto){ return i; };
    return lambda();
}
static_assert(copy_capture(2) == 2);

constexpr int init_capture(int i) {
    auto lambda = [x=i](this auto){ return x; };
    return lambda();
}
static_assert(init_capture(3) == 3);
