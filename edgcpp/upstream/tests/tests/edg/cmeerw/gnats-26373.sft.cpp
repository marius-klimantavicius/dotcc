//type:fp
//options:--c++20:--ms_c++20:--c++20 --clang_version 160000

namespace minimal
{
  struct B {
    B(const B&);
    template<typename T> requires false
    B(T &&);
  };
  struct D : public B { };
}

namespace tmpl_ctor
{
  struct B
  {
    B() { }
    B(const B&) { }

    template<typename T> requires false
    B(T &&);
  };

  struct D : public B
  {
    D();
  };

  void f(D d)
  {
    D dd(static_cast<D &&>(d));
  }
}

namespace tmpl_ctor_trailing_requires
{
  struct B
  {
    B() { }
    B(const B&) { }

    template<typename T>
    B(T &&) requires false;
  };

  struct D : public B
  {
    D();
  };

  void f(D d)
  {
    D dd(static_cast<D &&>(d));
  }
}

namespace tmpl_assign
{
  struct B
  {
    B() { }

    B &operator =(const B&)
    { return *this; }

    template<typename T> requires false
    B &operator =(T &&);
  };

  struct D : public B
  {
    D();
  };

  void f(D d)
  {
    d = D();
  }
}

namespace tmpl_assign_trailing_requires
{
  struct B
  {
    B() { }

    B &operator =(const B&)
    { return *this; }

    template<typename T>
    B &operator =(T &&) requires false;
  };

  struct D : public B
  {
    D();
  };

  void f(D d)
  {
    d = D();
  }
}

#if _MSC_VER
// MSVC only checks constraints on copy/move constructors
namespace late_checking_of_tmpl_constraint_for_msvc
{
  template<typename T>
  struct C
  {
    static_assert(sizeof(T) == 0);
    static constexpr bool value = false;
  };

  template<typename U>
  struct B
  {
    B() { }

    B(const B&);

    template<typename T> requires C<T>::value
    B(T);
  };

  struct D : public B<int>
  {
    D();
  };

  void f(D d)
  {
    d = D();
    D dd(static_cast<D &&>(d));
  }
}

namespace late_checking_of_tmpl_trailing_constraint_for_msvc
{
  template<typename T>
  struct C
  {
    static_assert(sizeof(T) == 0);
    static constexpr bool value = false;
  };

  template<typename U>
  struct B
  {
    B() { }

    B(const B&);

    template<typename T>
    B(T) requires C<T>::value;
  };

  struct D : public B<int>
  {
    D();
  };

  void f(D d)
  {
    d = D();
    D dd(static_cast<D &&>(d));
  }
}
#endif

namespace dont_instantiate_with_unsatisfied_constraints
{
  struct B
  {
    B(const B&);

    template<typename T>
    B(T &&) requires false
    {
      T::value;
    }
  };
  struct D : public B { };

  void f(D d)
  {
    D dd(static_cast<D &&>(d));
  }
}
