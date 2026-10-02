//type:fp
//options:--c++20:--c++20 --gn 140200:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<bool B> struct N {
    template<typename T> struct C {
      C(T) requires B;
    };
  };
  N<true>::C c(0);
}

namespace nested_classes
{
  template<bool>
  struct A
  { };

  template<bool B1>
  struct C
  {
    template<bool B2>
    struct D
    {
      D(A<B2>) requires B1 && B2;

      template<bool B3>
      D(A<B2>, A<B3>) requires B1 && B2 && B3;
    };
  };

  C<true>::D d1{A<true>()};
  C<true>::D d2{A<true>(), A<true>()};
}
