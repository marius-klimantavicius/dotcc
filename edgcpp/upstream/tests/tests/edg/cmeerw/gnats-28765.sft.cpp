//type:fp
//options:--c++14:--c++20:--c++20 --gn 150200:--c++20 --clang_version 220100:--ms_c++20 --microsoft_version 1950
//options_all:-w

namespace minimal
{
  template<typename T>
  int f(T t) noexcept(noexcept(t(1)));
  struct C {
    using type = int;
    friend int g(C) {
      return f([] (auto a) {
        return type{a};  // Previously a spurious error, now okay.
      });
    }
  };
  int i = g(C());
}

namespace generic_lambda
{
  template<typename T>
  int f(T t) noexcept(noexcept(t(1)));

  namespace ns1
  {
    using type1 = int *;
    using type2 = int;

    struct C
    {
      using type1 = int;
      using type2 = int *;
      friend int g1(C)
      {
        return f([](auto a) { return type1{a}; });
      }

      friend int g2(C);
    };

    int g2(C) {
      return f([](auto a) { return type2{a}; });
    }
  }

  namespace ns2
  {
    using type1 = int *;
    using type2 = int;

    template<class T>
    struct C
    {
      using type1 = int;
      using type2 = int *;

      friend int g1(C)
      {
        return f([](auto a) { return type1{a}; });
      }

      friend int g2(C);
    };

    int g2(C<void>)
    {
      return f([](auto a) { return type2{a}; });
    }
  }

  void test()
  {
    g1(ns1::C());
    g2(ns1::C());
    g1(ns2::C<int>());
    g2(ns2::C<void>());
  }
}

namespace nongeneric_lambda
{
  template<typename T>
  int f(T t) noexcept(noexcept(t(1)));

  namespace ns1
  {
    using type1 = int *;
    using type2 = int;

    struct C
    {
      using type1 = int;
      using type2 = int *;
      friend int g1(C)
      {
        return f([](int a) { return type1{a}; });
      }

      friend int g2(C);
    };

    int g2(C) {
      return f([](int a) { return type2{a}; });
    }
  }

  namespace ns2
  {
    using type1 = int *;
    using type2 = int;

    template<class T>
    struct C
    {
      using type1 = int;
      using type2 = int *;

      friend int g1(C)
      {
        return f([](int a) { return type1{a}; });
      }

      friend int g2(C);
    };

    int g2(C<void>)
    {
      return f([](int a) { return type2{a}; });
    }
  }

  void test()
  {
    g1(ns1::C());
    g2(ns1::C());
    g1(ns2::C<int>());
    g2(ns2::C<void>());
  }
}

namespace ns
{
  template<typename T>
  int f(T t) noexcept(noexcept(t(0)));

  struct A {
    using type = int;
    friend auto get(A)
    {
      return [](auto x) { return type{x}; };
    }
  };

  struct B {
    using type = char*;
    friend auto get(A);
  };

  void test()
  {
    f(get(A{}));
  }
}
