//type:fp
//options:--c++11 --gn 150200

namespace minimal
{
  template<int ... Is>
  struct C {
    struct D { };
  };

  template<int I>
  using A = typename C<__integer_pack(I)...>::D;
}

namespace dpdt
{
  template<typename T, T ... vs>
  struct C
  {
    struct D { };
  };

  template<int I>
  using A = typename C<int, __integer_pack(I)...>::D;

  using A3 = A<3>;
  using A3 = C<int, 0, 1, 2>::D;
}

namespace non_dpdt
{
  template<typename T, T ... vs>
  struct C
  {
    struct D { };
  };

  using D3 = C<int, __integer_pack(3) ...>::D;
  using D3 = C<int, 0, 1, 2>::D;
}
