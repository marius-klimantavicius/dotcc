//type:fn
//options:--c++20:--ms_c++20

namespace multiple_inheritance_constrained
{
  template<typename T>
  struct C
  {
    static_assert(T::value);
  };

  template<typename T>
  struct B1
  {
    B1() requires true
    { }
  };

  template<typename T>
  struct B2
  {
    B2() requires true
    { }
  };

  template <typename T>
  struct D
    : B1<T>, B2<T>
  {
    D() requires false
    { }

    using B1<T>::B1;
    using B2<T>::B2;
  };

  D<int> d;                     // ambiguous constructor call
}

namespace multiple_inheritance_unconstrained
{
  template<typename T>
  struct C
  {
    static_assert(T::value);
  };

  template<typename T>
  struct B1
  {
    B1()
    { }
  };

  template<typename T>
  struct B2
  {
    B2()
    { }
  };

  template <typename T>
  struct D
    : B1<T>, B2<T>
  {
    D() requires false
    { }

    using B1<T>::B1;
    using B2<T>::B2;
  };

  D<int> d;                     // ambiguous constructor call
}

namespace different_requires_clauses
{
  template<typename T>
  struct B
  {
    static constexpr int f() requires (sizeof(T) >= sizeof(char))
    {
      return 0;
    }
  };

  template<typename T>
  struct D : B<T>
  {
    using B<T>::f;

    static constexpr int f() requires (sizeof(T) == sizeof(char))
    {
      return 1;
    }
  };

  static_assert(D<char>::f() == 1);
}
