//type:fn
//options:;cp:--gnu_version 40800:--gnu_version 40900:--clang_version 80000:--clang_version 80100;cp:--microsoft_version 1900:--microsoft_version 1910
//options_all:--c++11

struct T {
  T();
  T(T &&) noexcept(false);
};
struct U {
  T t;
  U();
  U(U &&) noexcept = default;
};
U u1;
U u2 = static_cast<U&&>(u1);
