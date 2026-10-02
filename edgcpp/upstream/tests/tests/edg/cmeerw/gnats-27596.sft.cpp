//type:fp
//options:--c++20

namespace minimal
{
  template<typename> concept X = true;
  template<typename T> struct C {
    template<typename U> void f(U) requires X<T>;
    void g(int) requires X<T>;
  };
  template<> template<typename U>
  void C<int>::f(U) requires X<int>;
  template<>
  void C<int>::g(int);
}

namespace redecl_and_definition
{
  template<typename> concept X = true;

  template<typename T> struct C
  {
    template<typename U>
    void f(U) requires X<T>;

    void g(int) requires X<T>;
  };

  template<typename U>
  void h(U) requires X<U>;

  template<>
  void h(int);

  template<>
  void h(long)
  { }

  template<> template<typename U>
  void C<int>::f(U) requires X<int>;

  template<> template<typename U>
  void C<long>::f(U) requires X<long>
  { }

  template<>
  void C<int>::g(int);

  template<>
  void C<long>::g(int)
  { }
}

namespace overload_resolution_member
{
  template<typename T> concept X = sizeof(T) == 1;

  template<typename T> struct C
  {
    constexpr static int f()
    { return -1; }
    constexpr static int f() requires X<T>
    { return -1; }
    constexpr static int f() requires X<T> && X<T *>
    { return -1; }

    constexpr static int g()
    { return -1; }
    constexpr static int g() requires X<T> && X<T *>
    { return -1; }
    constexpr static int g() requires X<T>
    { return -1; }
  };

  template<>
  constexpr int C<char>::f()
  { return 1; }

  template<>
  constexpr int C<char>::g()
  { return 1; }

  static_assert(C<char>::f() == 1);
  static_assert(C<char>::g() == 1);
}

namespace overload_resolution_member_reversed
{
  template<typename T> concept X = sizeof(T) == 1;

  template<typename T> struct C
  {
    constexpr static int f() requires X<T>
    { return -1; }
    constexpr static int f() requires X<T> && X<T *>
    { return -1; }
    constexpr static int f()
    { return -1; }

    constexpr static int g() requires X<T> && X<T *>
    { return -1; }
    constexpr static int g() requires X<T>
    { return -1; }
    constexpr static int g()
    { return -1; }
  };

  template<>
  constexpr int C<char>::f()
  { return 1; }

  template<>
  constexpr int C<char>::g()
  { return 1; }

  static_assert(C<char>::f() == 1);
  static_assert(C<char>::g() == 1);
}

namespace overload_resolution_non_member
{
  template<typename T> concept X = sizeof(T) == 1;

  template<typename T>
  constexpr static int f(T)
  { return -1; }
  template<typename T>
  constexpr static int f(T) requires X<T>
  { return -1; }
  template<typename T>
  constexpr static int f(T) requires X<T> && X<T *>
  { return -1; }

  template<typename T>
  constexpr static int g(T)
  { return -1; }
  template<typename T>
  constexpr static int g(T) requires X<T> && X<T *>
  { return -1; }
  template<typename T>
  constexpr static int g(T) requires X<T>
  { return -1; }

  template<>
  constexpr int f(int)
  { return 1; }

  template<>
  constexpr int g(int)
  { return 1; }

  static_assert(f(1) == 1);
  static_assert(g(1) == 1);

  template<>
  constexpr int f(char)
  { return 1; }

  template<>
  constexpr int g(char)
  { return 1; }

  static_assert(f('a') == 1);
  static_assert(g('a') == 1);
}

namespace overload_resolution_non_member_reversed
{
  template<typename T> concept X = sizeof(T) == 1;

  template<typename T>
  constexpr static int f(T) requires X<T>
  { return -1; }
  template<typename T>
  constexpr static int f(T) requires X<T> && X<T *>
  { return -1; }
  template<typename T>
  constexpr static int f(T)
  { return -1; }

  template<typename T>
  constexpr static int g(T) requires X<T> && X<T *>
  { return -1; }
  template<typename T>
  constexpr static int g(T) requires X<T>
  { return -1; }
  template<typename T>
  constexpr static int g(T)
  { return -1; }

  template<>
  constexpr int f(int)
  { return 1; }

  template<>
  constexpr int g(int)
  { return 1; }

  static_assert(f(1) == 1);
  static_assert(g(1) == 1);

  template<>
  constexpr int f(char)
  { return 1; }

  template<>
  constexpr int g(char)
  { return 1; }

  static_assert(f('a') == 1);
  static_assert(g('a') == 1);
}

namespace overload_resolution_non_member_ref
{
  template<typename T> concept X = sizeof(T) == 1;

  template<typename T>
  constexpr static int f(T)
  { return -1; }
  template<typename T>
  constexpr static int f(T) requires X<T>
  { return -1; }
  template<typename T>
  constexpr static int f(T) requires X<T> && X<T &>
  { return -1; }

  template<typename T>
  constexpr static int g(T)
  { return -1; }
  template<typename T>
  constexpr static int g(T) requires X<T> && X<T &>
  { return -1; }
  template<typename T>
  constexpr static int g(T) requires X<T>
  { return -1; }

  template<>
  constexpr int f(int)
  { return 1; }

  template<>
  constexpr int g(int)
  { return 1; }

  static_assert(f(1) == 1);
  static_assert(g(1) == 1);

  template<>
  constexpr int f(char)
  { return 1; }

  template<>
  constexpr int g(char)
  { return 1; }

  static_assert(f('a') == 1);
  static_assert(g('a') == 1);
}

namespace overload_resolution_non_member_ref_reversed
{
  template<typename T> concept X = sizeof(T) == 1;

  template<typename T>
  constexpr static int f(T) requires X<T>
  { return -1; }
  template<typename T>
  constexpr static int f(T) requires X<T> && X<T &>
  { return -1; }
  template<typename T>
  constexpr static int f(T)
  { return -1; }

  template<typename T>
  constexpr static int g(T) requires X<T> && X<T &>
  { return -1; }
  template<typename T>
  constexpr static int g(T) requires X<T>
  { return -1; }
  template<typename T>
  constexpr static int g(T)
  { return -1; }

  template<>
  constexpr int f(int)
  { return 1; }

  template<>
  constexpr int g(int)
  { return 1; }

  static_assert(f(1) == 1);
  static_assert(g(1) == 1);

  template<>
  constexpr int f(char)
  { return 1; }

  template<>
  constexpr int g(char)
  { return 1; }

  static_assert(f('a') == 1);
  static_assert(g('a') == 1);
}
