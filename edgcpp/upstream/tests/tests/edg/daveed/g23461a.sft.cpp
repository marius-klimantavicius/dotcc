//Remark:enable_if attribute
//options:--clang --c++11;fp

namespace enable_if_attrs {
constexpr int fn1() __attribute__((enable_if(0, ""))) { return 0; }
constexpr int fn1() { return 1; }
}

static_assert(enable_if_attrs::fn1() == 1, "");
