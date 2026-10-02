//type:fn
//options:--c++17 -A:--ms_c++17 --microsoft_version 1926:--ms_c++20 --microsoft_version 1936:--c++17 --clang_version 170000
//options_all:-w -tused

namespace minimal
{
  struct C1 {
    C1(const C1 &);
    C1(int);
  };
  struct C2 {
    C2(C2 &&);
    C2(int);
  };
  struct D {
    operator C1() const;
    operator C2() const;
    operator int() const;
  };
  C1 c1{D{}};
  C2 c2{D{}};
}

namespace copy_ctor
{
  struct C
  {
    C(const C &);
    C(int);
  };

  struct D
  {
    operator C() const;
    operator int() const;
  };

  void f(D d)
  {
    C c1 = d;
    C c2(d);                    // error: strict
    C c3{d};                    // error: strict
    C c4 = { d };               // error: strict
    C c5{{d}};                  // OK
  }

  C return_arg(bool b, D d)
  {
    if (b) {
      return d;
    } else {
      return { d };             // error: strict
    }
  }
}

namespace move_ctor
{
  struct C
  {
    C(C &&);
    C(int);
  };

  struct D
  {
    operator C() const;
    operator int() const;
  };

  void f(D d)
  {
    C c1 = d;
    C c2(d);                    // error: strict, MSVC
    C c3{d};                    // error: strict, MSVC
    C c4 = { d };               // error: strict, MSVC
    C c5{{d}};                  // OK
  }

  C return_arg(bool b, D d)
  {
    if (b) {
      return d;
    } else {
      return { d };             // error: strict, MSVC
    }
  }
}

namespace direct_and_copy_init
{
  struct D
  {
    constexpr D()
      : v(-1)
    { }

    constexpr D(const D &d)
      : v(d.v)
    { }

    constexpr D(char)
      : v(1)
    { }

    int v;
  };

  struct S
  {
    constexpr operator char() const
    { return ' '; }

    constexpr operator D() const
    { return { }; }
  };

  constexpr S s{};
  static_assert( ((D) s).v == -1 ); // error: strict
  static_assert( (static_cast<D>(s)).v == -1 ); // error: strict

  constexpr int use_direct_init()
  {
    D d(s);                     // error: strict
    return d.v;
  }

  constexpr int use_direct_list_init()
  {
    D d{s};                     // error: strict
    return d.v;
  }

  constexpr int use_copy_init()
  {
    D d = s;
    return d.v;
  }

  static_assert(use_direct_init() == -1);
  static_assert(use_direct_list_init() == -1);
  static_assert(use_copy_init() == -1);
}

namespace tmpl_conversion_move_ctor
{
  struct X
  { };

  struct A
  {
    A(A&&);
    A(X) = delete;
  };

  struct B
  {
    template<class T> operator T();
  };

  B g;
  A v = g;
  A v1( g );                    // error: strict, MSVC
  A v2{ g };                    // error: strict, MSVC
  A v3 = { g };                 // error: strict, MSVC
}

namespace non_tmpl_conversion_copy_ctor
{
  struct X
  { };

  struct A
  {
    A(const A &);
    A(X) = delete;
  };

  struct B
  {
    operator A();
    operator X();
  };

  B g;
  A v = g;
  A v1( g );                    // error: strict
  A v2{ g };                    // error: strict
  A v3 = { g };                 // error: strict
}

namespace non_tmpl_conversion_move_ctor
{
  struct X
  { };

  struct A
  {
    A(A&&);
    A(X) = delete;
  };

  struct B
  {
    operator A();
    operator X();
  };

  B g;
  A v = g;
  A v1( g );                    // error: strict, MSVC
  A v2{ g };                    // error: strict, MSVC
  A v3 = { g };                 // error: strict, MSVC
}

namespace copy_list_init
{
  struct D
  {
    constexpr D()
      : v(-1)
    { }

    constexpr D(const D &d)
      : v(d.v)
    { }

    constexpr D(char)
      : v(1)
    { }

    int v;
  };

  struct S
  {
    constexpr operator char() const
    { return ' '; }

    constexpr operator D() const
    { return { }; }
  };

  constexpr S s{};

  constexpr int use_copy_list_init()
  {
    D d = { s };                // error: strict
    return d.v;
  }

  static_assert(use_copy_list_init() == -1);
}

namespace aggr_init
{
  struct Y {
    explicit Y(int i) { }
  };

  struct S {
    Y y;
  };

  struct U {
    operator int() const;
    operator Y() const;
  };

  S s{ { U{} } };                 // error: strict, MSVC

  void f(S s)
  {
    s = { U{} };
    s = { { U{} } };            // error: strict, MSVC
    s.y = { U{} };              // error: strict, MSVC
    s.y = { { U{} } };          // error: strict, MSVC, clang

    S{ U{} };
    S{ { U{} } };               // error: strict, MSVC
  }
}

namespace ref_udc
{
  struct C
  {
    C(C &&);
    C(const C &);
    C(int);
  };

  struct D1
  {
    operator int() const;
    operator C &() const;
  };

  struct D2
  {
    operator int() const;
    operator const C &() const;
  };

  C c1{D1{}};                   // error: strict, clang
  C c2{D2{}};                   // error: strict, clang
}
