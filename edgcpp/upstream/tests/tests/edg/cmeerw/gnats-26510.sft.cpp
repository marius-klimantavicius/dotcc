//type:fp
//options:--c++20 -A:--c++20 --gn 120100:--c++20 --clang_version 170000:--c++20 --clang_version 180100:--ms_c++20 --microsoft_version 1934

namespace minimal
{
#if !defined(__clang__) || (__clang_major__ >= 18)
  template<bool B>
  struct A {
    static_assert(B);
  };
  struct C {
    template<typename T> requires (sizeof(T) != sizeof(int))
    explicit(A<sizeof(T) != sizeof(int)>::v) C(T);
    C(long);
  };
  C c = 1;
#endif
}

template<typename T, bool B>
struct X
{
  static_assert(B);

  static constexpr bool value = true;
  using type = int;
};

#if defined(__clang__) && (__clang_major__ < 18)
namespace subst_explicit_conditional_first
{
  template<typename T>
  struct C
  {
    template<typename U> requires X<U, false>::value
    explicit(!X<U, true>::fail) C(typename X<U, false>::type, U);

    C(long, long);
  };

  // we don't emulate that yet
  //C<int> c(1, 2);
}
#elif defined(_MSC_VER) || defined(__clang__)
namespace subst_param_first
{
  template<typename T>
  struct C
  {
    template<typename U> requires X<U, false>::value
    explicit(!X<U, false>::value) C(typename X<U, true>::fail, U);

    C(long, long);
  };

  C<int> c(1, 2);
}
#else
namespace subst_requires_first
{
  template<typename T>
  struct C
  {
    template<typename U> requires (!X<U, true>::value)
    explicit(!X<U, false>::value) C(typename X<U, false>::type, U);

    C(long, long);
  };

  C<int> c(1, 2);
}
#endif

#if defined(_MSC_VER) || (defined(__clang__) && (__clang_major__ >= 18))
// GCC would also do that, but we don't emulate that yet
namespace subst_param_before_explicit
{
  template<typename T>
  struct C
  {
    template<typename U>
    explicit(!X<U, false>::value) C(typename X<U, true>::fail, U);

    C(long, long);
  };

  C<int> c(1, 2);
}
#elif (!defined(_MSC_VER) && !defined(__GNUC__)) || defined(__clang__)
namespace subst_explicit_before_param
{
  template<typename T>
  struct C
  {
    template<typename U>
    explicit(!X<U, true>::fail) C(typename X<U, false>::type, U);

    C(long, long);
  };

  // we don't implement that yet
  //C<int> c(1, 2);
}
#endif

#if defined(_MSC_VER) || defined(__clang__)
namespace subst_param_before_requires
{
  template<typename T>
  struct C
  {
    template<typename U> requires X<U, false>::value
    C(typename X<U, true>::fail, U);

    C(long, long);
  };

  C<int> c(1, 2);
}
#endif

#if defined(_MSC_VER) || (defined(__GNUC__) && !defined(__clang__))
namespace subst_requires_before_explicit
{
  template<typename T>
  struct C
  {
    template<typename U> requires X<U, true>::fail
    explicit(!X<U, false>::value) C(typename X<U, true>::type, U);

    C(long, long);
  };

  C<int> c(1, 2);
}
#endif

namespace check_constraints_on_address_of_function
{
  template<typename T> int f() requires false;
  template<typename T> char f();

  auto (*p)() = &f<int>;
}

#if !defined(__clang__)
namespace ignore_simple_template_parameter_for_copy_move_ctor
{
  template<typename T>
  using A = T;

  template<typename T>
  struct C
  {
    static_assert(sizeof(T) == 0);
    static constexpr bool value = true;
  };

  template<typename U>
  struct B1
  {
    B1() { }
    B1(const B1&);

    template<typename T> requires C<T>::value
    B1(A<T>);
  };

  template<typename U>
  struct B2
  {
    B2() { }
    B2(const B2&);

    template<typename T> requires C<T>::value
    B2(A<T>);
  };

  struct D1 : public B1<int>
  {
    D1();
  };

  struct D2 : public B2<int>
  {
    D2();
  };

  void f(D1 d1, D2 d2)
  {
    {
      D1 d(static_cast<D1 &&>(d1));
    }

    {
      D2 d(static_cast<D2 &&>(d2));
    }
  }
}
#endif
