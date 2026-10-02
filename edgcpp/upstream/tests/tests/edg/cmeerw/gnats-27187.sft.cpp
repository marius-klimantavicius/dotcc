//type:fp
//options:--c++:--c++11 -DCPP11:--c++11 --gn 130200 -DCPP11:--c++11 --clang_version 180100 -DCPP11:--ms_c++20 --microsoft_version 1936 -DCPP11
//options_all:-w

namespace minimal
{
  template<int I> struct C2;
  template<int I>
  struct C1 {
    friend int f(C1, C2<I>) { return 1; }
  };
  template<int I>
  struct C2 {
    friend int f(C1<I>, C2);
  };
  int i = f(C1<0>(), C2<0>());
}

#if CPP11
namespace definition_and_declaration_simple
{
  template<int I> struct C2;
  template<int I>
  struct C1 {
    friend constexpr int f(C1, C2<I>) { return 1; }
  };
  template<int I>
  struct C2 {
    friend constexpr int f(C1<I>, C2);
  };
  static_assert(f(C1<0>(), C2<0>()) == 1, "Unexpected");
}

namespace declaration_and_definition_simple
{
  template<int I> struct C2;
  template<int I>
  struct C1 {
    friend constexpr int f(C1, C2<I>);
  };
  template<int I>
  struct C2 {
    friend constexpr int f(C1<I>, C2) { return 1; }
  };
  static_assert(f(C1<0>(), C2<0>()) == 1, "Unexpected");
}

namespace declaration_and_definition
{
  template<const int &>
  struct C1
  {
    friend constexpr int f(C1);
  };

  template<typename>
  struct C2
  {
    static constexpr int v = 1;
    friend constexpr int f(C1<v>) { return 1; }
  };

  constexpr C1<C2<int>::v> c;
  constexpr int i = f(c);

  static_assert(i == 1, "Unexpected");
}

namespace definition_and_declaration
{
  template<const int &>
  struct C1
  {
    friend constexpr int f(C1) { return 1; }
  };

  template<typename>
  struct C2
  {
    static constexpr int v = 1;
    friend constexpr int f(C1<v>);
  };

  constexpr C1<C2<int>::v> c;
  constexpr int i = f(c);

  static_assert(i == 1, "Unexpected");
}
#endif
