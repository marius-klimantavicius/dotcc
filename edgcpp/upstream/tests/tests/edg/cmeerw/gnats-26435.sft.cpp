//tpye:fp
//options:--c++20:--ms_c++20

namespace minimal
{
  template<int> concept C = false;
  template<typename T, int I = 0>
  struct D {
    D(T) requires (!C<I>);
  };
  D d(0);
}

namespace minimal_2
{
  template<typename, int> concept C = false;
  template<typename T, int K = 0>
  struct D {
    D(T) requires (!C<T, K>);
  };
  D d{ 1 };
}

namespace unary_operator
{
  template<typename T, int K>
  concept C = false;

  template<typename T, int K = 0>
  struct D {
    D(T) requires (!C<T, K>);
  };

  D d{ 1 };
}

namespace binary_operator
{
  template<typename T, int K>
  concept C = false;

  template<typename T, int K = 0>
  struct D {
    D(T) requires (C<T, K> == false);
  };

  D d{ 1 };
}

namespace ternary_operator
{
  template<typename T, int K>
  concept C = false;

  template<typename T, int K = 0>
  struct D {
    D(T) requires (C<T, K> ? false : true);
  };

  D d{ 1 };
}

namespace fn_arg
{
  template<typename T, int K>
  concept C = false;

  constexpr bool f(bool b)
  {
    return !b;
  }

  template<typename T, int K = 0>
  struct D {
    D(T) requires (f(C<T, K>));
  };

  D d{ 1 };
}

namespace deduced
{
  template<int>
  concept C = false;

  template<int>
  struct B { };

  template<typename T, int I>
  struct D {
    D(T, B<I>) requires (!C<I>);
  };

  D d{ 1, B<1>{} };
}
