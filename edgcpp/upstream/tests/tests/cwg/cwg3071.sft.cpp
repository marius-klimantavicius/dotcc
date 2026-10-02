//type:fn
//options:--c++17
//options_all:-A -tused

namespace std {
  template <typename T> struct tuple_size;
}

struct S {
  int a;
};

template <> struct std::tuple_size<S> {
  static constexpr int value = -1;
};

auto [a, b] = S{};

//cwg: 3071
//title: Negative tuple_size in structured bindings
//meeting: Kona 11/25
//edg_status: Passes
