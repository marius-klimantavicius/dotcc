//type:fp
//options:--c++20
//options_all:--clang_version 220100 -w

template<typename, typename>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

typedef int __v16si __attribute__((__vector_size__(64)));

void f(__v16si v16si)
{
  {
    auto v = __builtin_elementwise_fshl(v16si, v16si, v16si);
    static_assert(is_same_v<decltype(v), __v16si>);
  }

  {
    auto v = __builtin_elementwise_fshr(v16si, v16si, v16si);
    static_assert(is_same_v<decltype(v), __v16si>);
  }

  {
    auto v = __builtin_elementwise_clzg(v16si);
    static_assert(is_same_v<decltype(v), __v16si>);
  }

  {
    auto v = __builtin_elementwise_clzg(v16si, v16si);
    static_assert(is_same_v<decltype(v), __v16si>);
  }

  {
    auto v = __builtin_elementwise_ctzg(v16si);
    static_assert(is_same_v<decltype(v), __v16si>);
  }

  {
    auto v = __builtin_elementwise_ctzg(v16si, v16si);
    static_assert(is_same_v<decltype(v), __v16si>);
  }
}
