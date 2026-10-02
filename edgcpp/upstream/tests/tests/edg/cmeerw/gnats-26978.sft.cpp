//type:fp
//options:--c++20:--c++20 --gn 130200:--c++20 --clang_version 170001:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<int I>
  struct C {};
  template<auto I>
  void f() {
    using A = C<+I>;
    static_assert(requires { A{}; });
  }
  template void f<1>();
}

namespace minimal_2
{
  template<auto I>
  void f() {
    using A = decltype(I);
    static_assert(requires { A{1}; });
  }
  template void f<1>();
}

namespace alias_in_function_scope
{
  template<typename T>
  struct C
  {
    static_assert(sizeof(T) == 0);
  };

  template<>
  struct C<int>
  {
    constexpr static int v = 0;
  };

  template<typename T, T I>
  void f()
  {
    using A = C<decltype(I)>;
    static_assert(requires { A::v; });
  }

  template void f<int, 1>();
}

namespace alias_in_function_scope_auto_param
{
  template<typename T>
  struct C
  {
    static_assert(sizeof(T) == 0);
  };

  template<>
  struct C<int>
  {
    constexpr static int v = 0;
  };

  template<auto I>
  void f()
  {
    using A = C<decltype(I)>;
    static_assert(requires { A::v; });
  }

  template void f<1>();
}

namespace alias_in_class_scope
{
  template<typename T>
  struct C
  {
    static_assert(sizeof(T) == 0);
  };

  template<>
  struct C<int>
  {
    constexpr static int v = 0;
  };

  template<typename T, T I>
  struct B
  {
    using A = C<decltype(I)>;
    static_assert(requires { A::v; });
  };

  template struct B<int, 1>;
}

namespace alias_in_class_scope_auto_param
{
  template<typename T>
  struct C
  {
    static_assert(sizeof(T) == 0);
  };

  template<>
  struct C<int>
  {
    constexpr static int v = 0;
  };

  template<auto I>
  struct B
  {
    using A = C<decltype(I)>;
    static_assert(requires { A::v; });
  };

  template struct B<1>;
}

namespace alias_in_function_scope_sizeof_expr
{
  template<int T>
  struct C
  {
    static_assert(sizeof(T) == 0);
  };

  template<>
  struct C<4> {
    constexpr static int v = 0;
  };

  template<typename T, T I>
  void f()
  {
    using A = C<sizeof(I)>;
    static_assert(requires { A::v; });
  };

  template void f<int, 4>();
}

namespace alias_in_function_scope_constant_expr
{
  template<int T>
  struct C
  {
    static_assert(sizeof(T) == 0);
  };

  template<>
  struct C<4> {
    constexpr static int v = 0;
  };

  template<typename T, T I>
  void f()
  {
    using A = C<+I>;
    static_assert(requires { A::v; });
  };

  template void f<int, 4>();
}
