//type:fp
//options: -A --c++26 --set_flag reflection

#include <meta>

struct S;
consteval std::size_t f(int p) {
  constexpr std::size_t r = std::meta::is_complete_type(^^S) ? 1 : 2;
  if (!std::meta::is_complete_type(^^S)) {
    std::meta::define_aggregate(^^S, {});
  }
  return (p > 0) ? f(p - 1) : r;
}

consteval {
  if (f(1) != 2) {
    throw;  // OK, not evaluated
  }
}

//cwg: 3162
//title: Evaluation context of manifestly constant-evaluated expressions
//meeting: Croydon 3/26
