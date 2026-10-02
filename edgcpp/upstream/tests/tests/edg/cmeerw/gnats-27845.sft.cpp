//type:fp
//options:--c++20:--c++20 --gn 140200:--c++20 --clang_version 190100:--ms_c++20 --microsoft_version 1942
//options_all:-w -tused

namespace minimal
{
  template<typename T>
  int c = sizeof(T);
  template<typename T>
  constexpr bool v = requires { c<T>; };
  static_assert(v<void>);
}

namespace non_dependent
{
  struct C
  {
    C() = delete;
  };

  template<typename T> T v;
  template<typename T> int c = sizeof(T);

  static_assert(requires { v<C>; });
  static_assert(requires { c<void>; });
  static_assert(sizeof(c<void>) == sizeof(int));
}

namespace dependent
{
  struct C
  {
    C() = delete;
  };

  template<typename T> T v;
  template<typename T> int c = sizeof(T);

  template<typename T, typename U> bool f(T, U *)
  {
    return sizeof(v<T>) == sizeof(C) &&
           sizeof(c<U>) == sizeof(int) &&
           requires {
             v<T>;
             c<U>;
           };
  }

  template bool f(C, void *);
}

namespace deduced_type
{
  template<typename T>
  auto c = T{};
  template<typename T>
  constexpr bool v = requires { c<T> = 1; };
  static_assert(v<int> && !v<void *>);
}

namespace constant_required
{
  template<typename T>
  struct C
  {
    int i;
  };

  template<typename T>
  constexpr C<int> var{ 1 };

  template<typename T> requires (var<T>.i != 0)
  struct B
  { };

  B<int> b;
}

namespace deduced_type
{
  template<typename T>
  constexpr auto var = 1;

  template<typename, typename>
  constexpr bool is_same_v = false;

  template<typename T>
  constexpr bool is_same_v<T, T> = true;

  template<typename T> requires is_same_v<decltype(var<T>), const int>
  struct B1
  { };

  template<typename T> requires (!is_same_v<decltype(var<T>), const long>)
  struct B2
  { };

  B1<int> b1;
  B2<int> b2;
}

namespace non_type_arg
{
  template<int>
  int g(int);

  template<int N>
  constexpr int var = N;

  template<int N = 0, class = decltype(g<var<N>>(0))>
  int f();

  int i = f();
}

namespace constraint
{
  template<int N>
  constexpr bool var = N != 0;

  template<int N = 1>
  int f() requires var<N>;

  int i = f();
}

#ifndef _MSC_VER
namespace new_in_non_type_arg
{
  template<int I>
  struct A
  { };

  template<typename T>
  auto f(int) -> A<*new T[](1)>;

  template<typename T>
  int f(long);

  template<typename T>
  auto g(int) -> decltype(A<*new T[](1)>());

  template<typename T>
  int g(long);

  int i = f<int>(0) +
          g<int>(0);
}
#endif
