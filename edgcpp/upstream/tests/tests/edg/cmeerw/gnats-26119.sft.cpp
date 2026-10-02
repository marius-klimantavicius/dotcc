//type:fp
//options:--c++20:--ms_c++20

namespace minimal {
  struct B { };
  template<typename T> struct D : B {
    D() requires (sizeof(T) > sizeof(char));
    using B::B;
  };
  D<char> d;
}

namespace trivial_default_ctor
{
  template<typename T>
  struct B
  { };

  template <typename T>
  struct D
    : B<T>
  {
    using B<T>::B;

    D() requires false;
  };

  D<int> d;
}

namespace user_declared_default_ctor
{
  template<typename T>
  struct B
  {
    B();
  };

  template <typename T>
  struct D
    : B<T>
  {
    using B<T>::B;

    D() requires false;
  };

  D<int> d;
}

namespace constrained_default_ctor
{
  template<typename T>
  struct B
  {
    constexpr B() requires true
    { }

    int i = 0;
  };

  template <typename T>
  struct D
    : B<T>
  {
    using B<T>::B;

    constexpr D() requires true
    {
      this->i = 1;
    }
  };

  static_assert(D<int>().i == 1);
}

namespace constrained_base_ctor
{
  template<typename T>
  struct B
  {
    constexpr B() requires true
    { }

    constexpr B(int) requires true
    { }

    int i = 0;
  };

  template <typename T>
  struct D
    : B<T>
  {
    using B<T>::B;

    constexpr D()
      : j(1)
    {
      this->i = 1;
    }

    constexpr D(int)
      : j(1)
    {
      this->i = 1;
    }

    int j = 0;
  };

  static_assert(D<int>().i == 0 && D<int>().j == 0);
  static_assert(D<int>(0).i == 0 && D<int>(0).j == 0);
}

namespace constrained_base_ctor_with_default_arg
{
  template<typename T>
  struct B
  {
    constexpr B(int = 0) requires true
    { }

    int i = 0;
  };

  template <typename T>
  struct D
    : B<T>
  {
    using B<T>::B;

    constexpr D()
      : j(1)
    {
      this->i = 1;
    }

    constexpr D(int)
      : j(1)
    {
      this->i = 1;
    }

    int j = 0;
  };

  static_assert(D<int>().i == 0 && D<int>().j == 0);
  static_assert(D<int>(0).i == 0 && D<int>(0).j == 0);
}

namespace default_arg_in_derived_ctor
{
  template<typename T>
  struct B
  {
    int i;
  };

  template <typename T>
  struct D
    : B<T>
  {
    using B<T>::B;

    constexpr D(int = 0)
      : j(1)
    {
      this->i = 1;
    }

    int j = 0;
  };

  static_assert(D<int>().i == 1 && D<int>().j == 1);
  static_assert(D<int>(0).i == 1 && D<int>(0).j == 1);
}

namespace dont_check_constraints_on_using_declaration
{
  template<typename T>
  struct C
  {
    static_assert(T::value);
  };

  template<typename T>
  struct B
  { };

  template <typename T>
  struct D
    : B<T>
  {
    constexpr D() requires C<T>::value;

    using B<T>::B;
  };
}

namespace multiple_inheritance
{
  template<typename T>
  struct C
  {
    static_assert(T::value);
  };

  template<typename T>
  struct B1
  {
    constexpr B1()
    { }
  };

  template<typename T>
  struct B2
  {
    constexpr B2() requires true
    { }

    int i = 0;
  };

  template <typename T>
  struct D
    : B1<T>, B2<T>
  {
    D() requires false
    { }

    using B1<T>::B1;
    using B2<T>::B2;

    int j = 0;
  };

  static_assert(D<int>().i == 0 && D<int>().j == 0);
}

namespace tmpl_ctor
{
  template<typename T>
  struct B
  {
    B() = default;

    template<typename U>
    constexpr B(U u) requires (sizeof(U) == 1)
      : i(2)
    { }

    int i = 0;
  };

  template <typename T>
  struct D
    : B<T>
  {
    using B<T>::B;

    template<typename U>
    constexpr D(U u)
      : j(1)
    {
      this->i = 1;
    }

    int j = 0;
  };

  static_assert(D<int>('a').i == 2 && D<int>('a').j == 0);
  static_assert(D<int>(0).i == 1 && D<int>(0).j == 1);
}

namespace tmpl_ctor_conflicts
{
  template<typename T>
  struct B
  {
    B() = default;

    template<typename U>
    constexpr B(U u) requires (sizeof(U) == 1)
      : i(2)
    { }

    int i = 0;
  };

  template <typename T>
  struct D
    : B<T>
  {
    using B<T>::B;

    template<typename U>
    constexpr D(U u)
      : j(1)
    {
      this->i = 1;
    }

    template<typename U>
    constexpr D(U u) requires (sizeof(U) == 1)
      : j(2)
    {
      this->i = 1;
    }

    int j = 0;
  };

  static_assert(D<int>('a').i == 1 && D<int>('a').j == 2);
  static_assert(D<int>(0).i == 1 && D<int>(0).j == 1);
}

namespace non_tmpl_fn
{
  template<typename T>
  struct B
  {
    constexpr int f() const requires (sizeof(T) == sizeof(int))
    {
      return 0;
    }

    constexpr int g() const
    {
      return 0;
    }
  };

  template <typename T>
  struct D
    : B<T>
  {
    using B<T>::f;
    using B<T>::g;

    constexpr int f() const
    {
      return 1;
    }

    constexpr int g() const requires (sizeof(T) == sizeof(int))
    {
      return 1;
    }
  };

  static_assert(D<int>().f() == 0);
  static_assert(D<char>().f() == 1);

  static_assert(D<int>().g() == 1);
  static_assert(D<char>().g() == 0);
}

namespace tmpl_fn
{
  template<typename T>
  struct B
  {
    template<typename U>
    constexpr int f(U) const requires (sizeof(U) == sizeof(int))
    {
      return 0;
    }

    template<typename U>
    constexpr int g(U) const
    {
      return 0;
    }
  };

  template <typename T>
  struct D
    : B<T>
  {
    using B<T>::f;
    using B<T>::g;

    template<typename U>
    constexpr int f(U) const
    {
      return 1;
    }

    template<typename U>
    constexpr int g(U) const requires (sizeof(U) == sizeof(int))
    {
      return 1;
    }
  };

  static_assert(D<int>().f(0) == 0);
  static_assert(D<char>().f('a') == 1);

  static_assert(D<int>().g(0) == 1);
  static_assert(D<char>().g('a') == 0);
}

namespace corresponding_decls
{
  template<typename T>
  struct B
  {
    constexpr B() = default;

    constexpr B(int) requires (sizeof(T) == sizeof(char))
      : i(1)
    { }

    constexpr int f() const requires (sizeof(T) == sizeof(char))
    {
      return 0;
    }

    int i = 0;
  };

  template<typename T>
  struct D : B<T>
  {
    using B<T>::B;
    using B<T>::f;

    constexpr D(int) requires (sizeof(T) == sizeof(char))
    { }

    constexpr int f() const requires (sizeof(T) == sizeof(char))
    {
      return 1;
    }
  };

  static_assert(D<char>(0).i == 0);
  static_assert(D<char>(0).f() == 1);
}
