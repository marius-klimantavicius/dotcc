//Remark:enable_if attribute
//options:--clang --c++11;fp

constexpr int g() __attribute((enable_if(0, ""))) { return 0; }
constexpr int g() { return 1; }

static_assert(g() == 1, "");
