//options_all:--c++20 -tused -A
typedef __EDG_SIZE_TYPE__ size_t;
template <typename T, size_t N>
constexpr auto array_size(T (&)[N]) -> size_t {
    return N;
}
void check(int const (&param)[3]) {
    int local[] = {1, 2, 3};
    constexpr auto s0 = array_size(local); // ok
    constexpr auto s1 = array_size(param); // this should be okay too
}
