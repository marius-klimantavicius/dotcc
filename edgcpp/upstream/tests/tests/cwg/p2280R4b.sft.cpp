//type:fn
//options_all:--c++20 -tused
typedef __EDG_SIZE_TYPE__ size_t;
template <typename T, size_t N>
constexpr size_t array_size(T (*)[N]) {
    return N;
}

void check(int const (*param)[3]) {
    constexpr auto s2 = array_size(param); // error
}
