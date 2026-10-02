//remark:__is_invocable and reference_wrapper
//options:--c++20 --gnu=160101;fp

namespace std {
  template<typename ... Ts> struct reference_wrapper {};
};

struct S {};
using X = std::reference_wrapper<S>;

static_assert(!__is_invocable(int S::*, X));
