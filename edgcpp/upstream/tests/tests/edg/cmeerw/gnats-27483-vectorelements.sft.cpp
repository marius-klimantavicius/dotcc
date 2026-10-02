//type:fp
//options:--clang_version 180100:--clang_version 190100
//options_all:--c++17

template<typename, typename>
constexpr bool is_same = false;

template<typename T>
constexpr bool is_same<T, T> = true;

using vec_t = int __attribute__((__vector_size__(16)));
vec_t vec;

auto v = __builtin_vectorelements(vec);

static_assert(is_same<decltype(v), decltype(sizeof 0)>);
