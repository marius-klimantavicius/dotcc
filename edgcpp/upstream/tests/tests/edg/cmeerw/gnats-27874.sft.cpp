//type:fp
//options:--c++17:--c++17 --gn 140200:--c++17 --clang_version 190100:--ms_c++20 --microsoft_version 1942

namespace minimal
{
  template<int I, int V> struct B { };
  template<int I>
  class C {
    static constexpr int v = I;
  public:
    C(int, B<I, v>);
  };
  C c{1, B<1, 1>{}};
}

namespace private_member
{
  template<int I, int V> struct B { };

  template<int I>
  struct C
  {
  private:
    static constexpr int v = I;

  public:
    C(int, B<I, v>);
  };

  C c{1, B<1, 1>{}};
}

namespace inner_class_friend
{
  template<int I, int V> struct B { };

  template<int I>
  struct C
  {
    class X
    {
      friend class C<1>;
      static constexpr int v = I;
    };

    C(int, B<I, X::v>);
  };

  C c{1, B<1, 1>{}};
}
