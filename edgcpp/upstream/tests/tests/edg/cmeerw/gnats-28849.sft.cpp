//type:fp
//options:--c++17 --gn 100100;fn:--c++17 --clang_version 190100:--c++20 --gn 100100:--c++20 --clang_version 190100:--c++17 -A;fn


static_assert(__cpp_deduction_guides == (__cplusplus >= 202003L) ? 201907L : 201703L, "Unexpected");

namespace alias_ctad
{
  template<typename T>
  struct C {
    C(T);
  };

  template<typename T>
  using A = C<T>;

  C c(1);
  A a(1);                       // accepted by GCC and Clang
}

namespace non_simple_alias_ctad
{
  template<typename T>
  struct C {
    C(T);
  };

  template<typename T = int>
  using A = C<T>;

  C c(1);
  A a(1);                       // accepted by Clang
}
