//remark:__builtin_elementwise_... interpreter support
//options:--clang_v=220000 --c++23;fp

using V4I = int __attribute((vector_size(16)));

constexpr V4I v1 = { 1, 2, 3 },
              v2 = { 4, 5, 6 };
constexpr int test_constexpr_max() {
    return __builtin_elementwise_max(v1, v2)[1];
}
static_assert(test_constexpr_max() == 5, "elementwise_max should be constexpr");

constexpr int test_constexpr_min() {
    return __builtin_elementwise_min(v1, v2)[1];
}
static_assert(test_constexpr_min() == 2, "elementwise_min should be constexpr");

