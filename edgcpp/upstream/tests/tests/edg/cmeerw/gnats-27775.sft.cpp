//type:fp
//options:--c++26:--c++26 --gn 160100:--c++26 --clang_version 220100:--c++26 --microsoft_version 1950 --no_ms_permissive:--c++26 --microsoft_version 1950 --ms_permissive
//options_all:-w -tused

namespace minimal
{
  auto l = [] (auto v) {
    auto [... b] = v;
    return (b + ...);
  };
}

static_assert(__cpp_structured_bindings >= 202411);

template<typename, typename>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;

template<int I>
struct R
{ };

namespace expansions
{
  struct C
  {
    R<1> r1;
    R<2> r2;
    R<3> r3;
    R<4> r4;
  };

  struct Empty
  { };

  template<typename T>
  void f(T t, C c)
  {
    {
      auto [ ... b, r1, r2, r3, r4 ] = c;
      static_assert(sizeof ... (b) == 0);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, ... b, r2, r3, r4 ] = c;
      static_assert(sizeof ... (b) == 0);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, r2, ... b, r3, r4 ] = c;
      static_assert(sizeof ... (b) == 0);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, r2, r3, ... b, r4 ] = c;
      static_assert(sizeof ... (b) == 0);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, r2, r3, r4, ... b ] = c;
      static_assert(sizeof ... (b) == 0);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ ... b, r2, r3, r4 ] = c;
      static_assert(sizeof ... (b) == 1);
      ( b , ... );
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, ... b, r3, r4 ] = c;
      static_assert(sizeof ... (b) == 1);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, r2, ... b, r4 ] = c;
      static_assert(sizeof ... (b) == 1);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, r2, r3, ... b ] = c;
      static_assert(sizeof ... (b) == 1);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
    }

    {
      auto [ ... b, r3, r4 ] = c;
      static_assert(sizeof ... (b) == 2);
      ( b , ... );
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, ... b, r4 ] = c;
      static_assert(sizeof ... (b) == 2);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, r2, ... b ] = c;
      static_assert(sizeof ... (b) == 2);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
    }

    {
      auto [ ... b, r4 ] = c;
      static_assert(sizeof ... (b) == 3);
      ( b , ... );
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, ... b ] = c;
      static_assert(sizeof ... (b) == 3);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
    }

    {
      auto [ ... b ] = c;
      static_assert(sizeof ... (b) == 4);
      ( b , ... );
    }

    {
      static auto [ ... b ] = c;
      static_assert(sizeof ... (b) == 4);
      ( b , ... );
    }

    {
      auto [ ... b, r1, r2, r3, r4 ] = t;
      static_assert(sizeof ... (b) == 0);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, ... b, r2, r3, r4 ] = t;
      static_assert(sizeof ... (b) == 0);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, r2, ... b, r3, r4 ] = t;
      static_assert(sizeof ... (b) == 0);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, r2, r3, ... b, r4 ] = t;
      static_assert(sizeof ... (b) == 0);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, r2, r3, r4, ... b ] = t;
      static_assert(sizeof ... (b) == 0);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ ... b, r2, r3, r4 ] = t;
      static_assert(sizeof ... (b) == 1);
      ( b , ... );
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, ... b, r3, r4 ] = t;
      static_assert(sizeof ... (b) == 1);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, r2, ... b, r4 ] = t;
      static_assert(sizeof ... (b) == 1);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, r2, r3, ... b ] = t;
      static_assert(sizeof ... (b) == 1);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
      static_assert(is_same_v<decltype(r3), R<3>>);
    }

    {
      auto [ ... b, r3, r4 ] = t;
      static_assert(sizeof ... (b) == 2);
      ( b , ... );
      static_assert(is_same_v<decltype(r3), R<3>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, ... b, r4 ] = t;
      static_assert(sizeof ... (b) == 2);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, r2, ... b ] = t;
      static_assert(sizeof ... (b) == 2);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
      static_assert(is_same_v<decltype(r2), R<2>>);
    }

    {
      auto [ ... b, r4 ] = t;
      static_assert(sizeof ... (b) == 3);
      ( b , ... );
      static_assert(is_same_v<decltype(r4), R<4>>);
    }

    {
      auto [ r1, ... b ] = t;
      static_assert(sizeof ... (b) == 3);
      ( b , ... );
      static_assert(is_same_v<decltype(r1), R<1>>);
    }

    {
      auto [ ... b ] = t;
      static_assert(sizeof ... (b) == 4);
      ( b , ... );
    }

    {
      static auto [ ... b ] = t;
      static_assert(sizeof ... (b) == 4);
      ( b , ... );
    }

    {
      auto [ ... b ] = Empty();
      static_assert(sizeof ... (b) == 0);
      ( b , ... );
    }

    {
      static auto [ ... b ] = Empty();
      static_assert(sizeof ... (b) == 0);
      ( b , ... );
    }
  }

  template void f(C, C);
}

namespace constant_evaluation
{
  struct C1
  {
    int i;
  };

  struct C2
  {
    int i;
    int j;
  };

  struct C3
  {
    int i;
    int j;
    int k;
  };

  constexpr int f(auto c)
  {
    auto [ ... b ] = c;
    return (b + ... );
  }

  static_assert(f(C1{ 1 }) == 1);
  static_assert(f(C2{ 1, 2 }) == 3);
  static_assert(f(C3{ 1, 2, 3 }) == 6);
}

namespace multiple_packs_in_same_scope
{
  struct C
  {
    int i;
    int j;
  };

  constexpr int f(C c1, auto c2)
  {
    auto [ ... b1 ] = c1;
    auto [ ... b2 ] = c2;

    if (sizeof ... (b1) != 0 && sizeof ... (b2) != 0)
    {
      return ( b1 + ... ) + ( b2 + ... ) + ( ( b1 + b2 ) + ... );
    }
    else
    {
      return -1;
    }
  }

  static_assert(f(C{ 1, 2 }, C{ 3, 4 }) == 20);
}

namespace multiple_static_packs_in_same_scope
{
  struct C
  {
    int i;
    int j;
  };

  int f(C c1, auto c2)
  {
    static auto [ ... b1 ] = c1;
    static auto [ ... b2 ] = c2;

    if (sizeof ... (b1) != 0 && sizeof ... (b2) != 0)
    {
      return ( b1 + ... ) + ( b2 + ... ) + ( ( b1 + b2 ) + ... );
    }
    else
    {
      return -1;
    }
  }

  template int f(C, C);
}

namespace lambda_capture
{
  struct C
  {
    int i;
    int j;
  };

  constexpr int f(C c1, auto c2)
  {
    auto [ ... b1 ] = c1;
    auto [ ... b2 ] = c2;

    return [&] () {
      return sizeof ... (b1) + sizeof ... (b2) +
             ( b1 + ... ) + ( b2 + ... ) + ( b1 + b2 + ... );
    } ();
  }

  static_assert(f(C{ 1, 2 }, C{ 3, 4 }) == 24);
}

namespace generic_lambda_capture
{
  struct C
  {
    int i;
    int j;
  };

  constexpr int f(C c1, auto c2)
  {
    auto [ ... b1 ] = c1;
    auto [ ... b2 ] = c2;

    return [&] (auto v) {
      return sizeof ... (b1) + sizeof ... (b2) +
             ( b1 + ... ) + ( b2 + ... ) + ( b1 + b2 + ... );
    } (1);
  }

  static_assert(f(C{ 1, 2 }, C{ 3, 4 }) == 24);
}

namespace for_range_loop
{
  struct C
  {
    int i;
    int b1, b2, b3;
    int j;
  };

  template<unsigned N>
  constexpr int f(auto (&arr)[N])
  {
    int sum = 0;
    for (auto [ i, ... b, j ] : arr)
    {
      sum += i + j;
      sum += (b + ...);
    }
    return sum;
  }

  constexpr C arr1[1] = {
      C{  1,  2,  3,  4,  5 }
  };
  static_assert(f(arr1) == 15);

  constexpr C arr2[2] = {
      C{  1,  2,  3,  4,  5 },
      C{ 11, 12, 13, 14, 15 }
  };
  static_assert(f(arr2) == 80);
}

namespace non_dpdt_init_for_pack
{
  struct C
  {
    char c;
    short s;
    int i;
    long l;
  };

  auto l = [] (auto v, C c) {
    {
      auto [ c0, ... b, l ] = c;
      static_assert(is_same_v<decltype(l), long>);
      long *p = &l;
    }

    {
      auto [ c0, ... b, l ] = v;
      static_assert(is_same_v<decltype(l), long>);
      long *p = &l;
    }

    return 0;
  } (C{ 'a', 2, 3, 4 }, C{ 'a', 2, 3, 4 });
}
