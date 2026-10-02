//type:fp
//options:--c++11:--c++20:--c++11 --gn 40804:--c++11 --gn 40900:--c++11 --gn 120100:--c++20 --gn 120100:--c++20 --clang_version 160000:--ms_c++20 --microsoft_version 1938

#if !defined(_MSC_VER) && !defined(__clang__) && \
    !(defined(__GNUC__) && ((__GNUC__*100 + __GNUC_MINOR__) < 409))
namespace minimal
{
  template<typename> class C;
  template<typename T> using A = C<T>;
  template<template<typename> class>
  class B { };
  B<A> b = B<C>{};
}
#endif

constexpr bool cwg1286 =
#if defined(_MSC_VER) || defined(__clang__) || \
    (defined(__GNUC__) && ((__GNUC__*100 + __GNUC_MINOR__) < 409))
  false;
#else
  true;
#endif

template<typename, typename>
struct is_same
{
  static constexpr bool value = false;
};

template<typename T>
struct is_same<T, T>
{
  static constexpr bool value = true;
};

namespace simple
{
  template<typename T>
  struct C;

  template<typename T>
  using A = C<T>;

  template<template<typename> class TT>
  struct B
  { };

  static_assert(cwg1286 == is_same<B<A>, B<C>>::value, "simple");
}

namespace simple_non_type
{
  template<int I>
  struct C;

  template<int I>
  using A = C<I>;

  template<template<int> class TT>
  struct B
  { };

  static_assert(cwg1286 == is_same<B<A>, B<C>>::value, "simple non type");
}

namespace simple_tmpl_tmpl
{
  template<template<typename> class>
  struct C;

  template<template<typename> class TT>
  using A = C<TT>;

  template<template<template<typename> class> class TT>
  struct B
  { };

  static_assert(cwg1286 == is_same<B<A>, B<C>>::value,
                "simple template template");
}

namespace double_alias
{
  template<typename T>
  struct C;

  template<typename T>
  using A = C<T>;

  template<typename T>
  using AA = A<T>;

  template<template<typename> class TT>
  struct B
  { };

  static_assert(cwg1286 == is_same<B<AA>, B<C>>::value, "double alias");
  static_assert(cwg1286 == is_same<B<AA>, B<A>>::value, "double alias");
}

namespace pack_param
{
  template<typename ... T>
  struct C;

  template<typename ... T>
  using A = C<T ...>;

  template<template<typename ...> class TT>
  struct B
  { };

  static_assert(cwg1286 == is_same<B<A>, B<C>>::value, "pack param");
}

namespace class_default_arg
{
  template<typename T = int>
  struct C;

  template<typename T>
  using A = C<T>;

  template<template<typename> class TT>
  struct B
  { };

  static_assert(!is_same<B<A>, B<C>>::value, "class default arg");
}

namespace alias_default_arg
{
  template<typename T>
  struct C;

  template<typename T = int>
  using A = C<T>;

  template<template<typename> class TT>
  struct B
  { };

  static_assert(!is_same<B<A>, B<C>>::value, "alias default arg");
}

namespace same_default_args
{
  template<typename T = int>
  struct C;

  template<typename T = int>
  using A = C<T>;

  template<template<typename> class TT>
  struct B
  { };

  static_assert(cwg1286 == is_same<B<A>, B<C>>::value, "same default args");
}

namespace different_default_args
{
  template<typename T = long>
  struct C;

  template<typename T = int>
  using A = C<T>;

  template<template<typename> class TT>
  struct B
  { };

  static_assert(!is_same<B<A>, B<C>>::value, "different default args");
}

namespace different_non_type_default_args
{
  template<int I = 1>
  struct C;

  template<int I = 2>
  using A = C<I>;

  template<template<int> class TT>
  struct B
  { };

  static_assert(!is_same<B<A>, B<C>>::value, "different non type default args");
}

namespace different_tmpl_tmpl_default_args
{
  template<typename>
  struct X1;

  template<typename>
  struct X2;

  template<template<typename> class = X1>
  struct C;

  template<template<typename> class TT = X2>
  using A = C<TT>;

  template<template<template<typename> class> class TT>
  struct B
  { };

  static_assert(!is_same<B<A>, B<C>>::value,
                "different template template default args");
}

namespace using_default_args
{
  template<typename T, typename U = long>
  struct C
  {
    static_assert(sizeof(U) == 1, "");
  };

  template<typename T, typename U = int>
  using A = C<T, U>;

  template<template<typename, typename = char> class TT>
  struct B
  {
    using type = TT<int>;
  };

  static_assert(!is_same<B<A>, B<C>>::value, "using default arg");
  static_assert(is_same<B<A>::type, B<C>::type>::value,
                "type using default arg");
}

namespace nested_template
{
  template<typename>
  struct C;

  template<typename T>
  using A = C<T>;

  template<template<typename> class>
  struct D
  { };

  template<template<typename> class TT>
  struct B
  {
    template<typename U>
    using XX = TT<U>;

    using type1 = D<XX>;
    using type2 = D<TT>;
  };

  static_assert(cwg1286 == is_same<B<A>::type1, B<A>::type2>::value,
                "nested template");
  static_assert(cwg1286 == is_same<B<A>::type1, B<C>::type2>::value,
                "nested template");
  static_assert(cwg1286 == is_same<B<C>::type1, B<A>::type2>::value,
                "nested template");
  static_assert(cwg1286 == is_same<B<C>::type1, B<C>::type2>::value,
                "nested template");
}

namespace aliased_nested_in_non_template
{
  template<template<typename> class C>
  struct B
  {
    template<typename U>
    using A = C<U>;
  };

  struct F
  {
    template<typename T>
    struct D
    {
      D(T);
    };
  };

  template<template<typename> class>
  struct X
  { };

  static_assert(cwg1286 == is_same<X<F::D>, X<B<F::D>::A>>::value,
                "aliased nested in non-template");
}

namespace aliased_nested_in_template
{
  template<template<typename> class C>
  struct B
  {
    template<typename U>
    using A = C<U>;
  };

  template<typename U>
  struct F
  {
    template<typename T>
    struct D
    {
      D(T);
    };

    template<typename T>
    using E = D<T>;

    template<typename T>
    using G = E<T>;
  };

  template<template<typename> class>
  struct X
  { };

  static_assert(!is_same<X<F<int>::D>, X<B<F<int>::D>::A>>::value,
                "aliased nested in non-template");
  static_assert(!is_same<X<F<int>::D>, X<F<int>::E>>::value,
                "aliased nested in non-template");
  static_assert(!is_same<X<F<int>::E>, X<F<int>::G>>::value,
                "aliased nested in non-template");
}

namespace compare_member_templates
{
  template<typename>
  struct A
  { };

  template<template<typename> class>
  struct D
  { };

  template<typename O>
  struct S
  {
    template<template<typename> class TT>
    struct B
    {
      template<typename U>
      using XX = TT<U>;

      template<typename U>
      using YY = TT<U *>;
    };

    void f(D<B<A>::template XX>);
    void f(D<B<A>::template YY>);
  };
}

#if __cpp_nontype_template_parameter_auto
namespace non_type_auto_parameter
{
  template<auto I>
  struct B
  { };

  template<auto I>
  using A = B<I>;

  template<template<auto> class C>
  struct X
  { };

  static_assert(cwg1286 == is_same<X<B>, X<A>>::value,
                "non-type auto parameter");
}
#endif

#if __cpp_concepts
namespace more_constrained_alias
{
  template<typename T>
  struct C
  { };

  template<typename T> requires true
  using A = C<T>;

  template<template<typename> class TT>
  struct B
  { };

  static_assert(!is_same<B<A>, B<C>>::value, "more constrained alias");
}

namespace less_constrained_alias
{
  template<typename T> requires true
  struct C
  { };

  template<typename T>
  using A = C<T>;

  template<template<typename> class TT>
  struct B
  { };

  static_assert(cwg1286 == is_same<B<A>, B<C>>::value,
                "less constrained alias");
}
#endif

namespace templated_member_class
{
  struct D
  { };

  template<typename T>
  struct C
  {
    struct B
    { };

    template<typename U>
    using A = B;
  };


  template<template<typename> class>
  struct X
  { };

  X<C<int>::A> x;
}
