//type:fp
//options:--c++20:--ms_c++20 --microsoft_version=1927

template<typename T1, typename T2>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;


namespace identical
{
  template<typename T>
  struct C
  {
    C(T);
  };

  template<typename U>
  using A = C<U>;

  C c1(1);
  static_assert(is_same_v<decltype(c1), C<int>>);

  A a1(1);
  static_assert(is_same_v<decltype(a1), C<int>>);
  static_assert(is_same_v<decltype(a1), A<int>>);

  C c2("");
  static_assert(is_same_v<decltype(c2), C<const char *>>);

  A a2("");
  static_assert(is_same_v<decltype(a2), C<const char *>>);
  static_assert(is_same_v<decltype(a2), A<const char *>>);

  void foo()
  {
    auto c1 = C{1};
    static_assert(is_same_v<decltype(c1), C<int>>);
    auto c2 = C(1);
    static_assert(is_same_v<decltype(c2), C<int>>);
    auto p1 = new C{1};
    static_assert(is_same_v<decltype(p1), C<int> *>);
    auto p2 = new C{1};
    static_assert(is_same_v<decltype(p2), C<int> *>);

    auto a1 = A{1};
    static_assert(is_same_v<decltype(a1), C<int>>);
    static_assert(is_same_v<decltype(a1), A<int>>);
    auto a2 = A(2);
    static_assert(is_same_v<decltype(a1), C<int>>);
    static_assert(is_same_v<decltype(a2), A<int>>);
    auto q1 = new A{1};
    static_assert(is_same_v<decltype(q1), C<int> *>);
    static_assert(is_same_v<decltype(q1), A<int> *>);
    auto q2 = new A{1};
    static_assert(is_same_v<decltype(q2), C<int> *>);
    static_assert(is_same_v<decltype(q2), A<int> *>);
  }
}

namespace class_nttp
{
  template<typename T>
  struct C
  {
    constexpr C(T v)
      : t(v)
    { }

    T t;
  };

  template<typename U>
  using A = C<U>;

  template<C c>
  struct X
  {
    decltype(c) m;
  };

  template<A a>
  struct Y
  {
    decltype(a) m;
  };

  constexpr X<1> x{1};
  static_assert(is_same_v<decltype(x), const X<C<int>{1}>>);
  static_assert(is_same_v<decltype(x.m), C<int>>);
  static_assert(x.m.t == 1);

  constexpr Y<1> y{1};
  static_assert(is_same_v<decltype(y), const Y<A<int>{1}>>);
  static_assert(is_same_v<decltype(y.m), A<int>>);
  static_assert(y.m.t == 1);
}

namespace identical_nontype
{
  template<int I>
  struct X
  { };

  template<int I>
  struct C
  {
    C(X<I>);
  };

  template<int I>
  using A = C<I>;

  C c1(X<1>{});
  static_assert(is_same_v<decltype(c1), C<1>>);

  A a1(X<1>{});
  static_assert(is_same_v<decltype(a1), C<1>>);
  static_assert(is_same_v<decltype(a1), A<1>>);

  C c2(X<2>{});
  static_assert(is_same_v<decltype(c2), C<2>>);

  A a2(X<2>{});
  static_assert(is_same_v<decltype(a2), C<2>>);
  static_assert(is_same_v<decltype(a2), A<2>>);
}

namespace typename_specifiers
{
  template<typename U>
  struct B
  {
    template<typename T>
    struct C
    {
      C(T);
    };

    template<typename V>
    using A = C<V>;
  };

  template<typename T>
  void foo()
  {
    typename B<T>::C c(1);
    static_assert(is_same_v<decltype(c), B<int>::C<int>>);

    typename B<T>::A a(1);
    static_assert(is_same_v<decltype(a), B<int>::C<int>>);
    static_assert(is_same_v<decltype(a), B<int>::A<int>>);
  }

  template void foo<int>();
}

namespace identical_tmpl_tmpl
{
  template<template<int> class TT>
  struct X
  { };

  template<int>
  struct Y
  { };

  template<template<int> class TT>
  struct C
  {
    C(X<TT>);
  };

  template<template<int> class TT>
  using A = C<TT>;

  C c1(X<Y>{});
  static_assert(is_same_v<decltype(c1), C<Y>>);

  A a1(X<Y>{});
  static_assert(is_same_v<decltype(a1), C<Y>>);
  static_assert(is_same_v<decltype(a1), A<Y>>);
}

namespace identical_explicit_guide
{
  template<typename T>
  struct C
  {
    C(T);
  };

  template<typename T>
  C(T) -> C<T>;

  template<typename U>
  using A = C<U>;

  C c1(1);
  static_assert(is_same_v<decltype(c1), C<int>>);

  A a1(1);
  static_assert(is_same_v<decltype(a1), C<int>>);
  static_assert(is_same_v<decltype(a1), A<int>>);

  C c2("");
  static_assert(is_same_v<decltype(c2), C<const char *>>);

  A a2("");
  static_assert(is_same_v<decltype(a2), C<const char *>>);
  static_assert(is_same_v<decltype(a2), A<const char *>>);
}

namespace multiple_identical
{
  template<typename T>
  struct C
  {
    C(T);
  };

  template<typename U>
  using B = C<U>;

  template<typename U>
  using A = B<U>;

  C c1(1);
  static_assert(is_same_v<decltype(c1), C<int>>);

  B b1(1);
  static_assert(is_same_v<decltype(b1), C<int>>);
  static_assert(is_same_v<decltype(b1), B<int>>);

  A a1(1);
  static_assert(is_same_v<decltype(a1), C<int>>);
  static_assert(is_same_v<decltype(a1), B<int>>);
  static_assert(is_same_v<decltype(a1), A<int>>);

  C c2("");
  static_assert(is_same_v<decltype(c2), C<const char *>>);

  B b2("");
  static_assert(is_same_v<decltype(b2), C<const char *>>);
  static_assert(is_same_v<decltype(b2), B<const char *>>);
  static_assert(is_same_v<decltype(b2), A<const char *>>);

  A a2("");
  static_assert(is_same_v<decltype(a2), C<const char *>>);
  static_assert(is_same_v<decltype(a2), A<const char *>>);
}

namespace multiple_identical_explicit_guide
{
  template<typename T>
  struct C
  {
    C(T);
  };

  template<typename T>
  C(T) -> C<T>;

  template<typename U>
  using B = C<U>;

  template<typename U>
  using A = B<U>;

  C c1(1);
  static_assert(is_same_v<decltype(c1), C<int>>);

  B b1(1);
  static_assert(is_same_v<decltype(b1), C<int>>);
  static_assert(is_same_v<decltype(b1), B<int>>);

  A a1(1);
  static_assert(is_same_v<decltype(a1), C<int>>);
  static_assert(is_same_v<decltype(a1), B<int>>);
  static_assert(is_same_v<decltype(a1), A<int>>);

  C c2("");
  static_assert(is_same_v<decltype(c2), C<const char *>>);

  B b2("");
  static_assert(is_same_v<decltype(b2), C<const char *>>);
  static_assert(is_same_v<decltype(b2), B<const char *>>);
  static_assert(is_same_v<decltype(b2), A<const char *>>);

  A a2("");
  static_assert(is_same_v<decltype(a2), C<const char *>>);
  static_assert(is_same_v<decltype(a2), A<const char *>>);
}

namespace pointer_to
{
  template<typename T>
  struct C
  {
    C(T);
  };

  template<typename U>
  using A = C<U *>;

  C c("");
  static_assert(is_same_v<decltype(c), C<const char *>>);

  A a("");
  static_assert(is_same_v<decltype(a), C<const char *>>);
  static_assert(is_same_v<decltype(a), A<const char>>);
}

namespace pointer_to_explicit_guide
{
  template<typename T>
  struct C
  {
    C(T);
  };

  template<typename T>
  C(T) -> C<T>;

  template<typename U>
  using A = C<U *>;

  C c("");
  static_assert(is_same_v<decltype(c), C<const char *>>);

  A a("");
  static_assert(is_same_v<decltype(a), C<const char *>>);
  static_assert(is_same_v<decltype(a), A<const char>>);
}

namespace nested_template
{
  template<typename S>
  struct B
  {
    template<typename T>
    struct C
    {
      C(T);
    };
  };

  template<typename U>
  using A = B<int>::C<U *>;

  B<long>::C c("");
  static_assert(is_same_v<decltype(c), B<long>::C<const char *>>);

  A a("");
  static_assert(is_same_v<decltype(a), B<int>::C<const char *>>);
  static_assert(is_same_v<decltype(a), A<const char>>);
}

namespace nested_template_explicit_guide
{
  template<typename S>
  struct B
  {
    template<typename T>
    struct C
    {
      C(...);
    };

    template<typename T>
    C(T) -> C<T>;
  };

  template<typename U>
  using A = B<int>::C<U *>;

  B<long>::C c("");
  static_assert(is_same_v<decltype(c), B<long>::C<const char *>>);

  A a("");
  static_assert(is_same_v<decltype(a), B<int>::C<const char *>>);
  static_assert(is_same_v<decltype(a), A<const char>>);
}

namespace nested_alias_explicit_guide
{
  template<typename ... S>
  struct B
  {
    template<typename ... T>
    struct C
    {
      C(...);
    };

    template<typename ... T>
    C(T ...) -> C<T ...>;

    template<typename ... U>
    using A = C<U ...>;
  };

  template<typename ... U>
  using BA = B<char>::C<U ...>;

  B<long>::C c("");
  static_assert(is_same_v<decltype(c), B<long>::C<const char *>>);

  B<int>::A a1("");
  static_assert(is_same_v<decltype(a1), B<int>::C<const char *>>);
  static_assert(is_same_v<decltype(a1), B<int>::A<const char *>>);

  B<int>::A a2("", (int *) 0);
  static_assert(is_same_v<decltype(a2), B<int>::C<const char *, int *>>);
  static_assert(is_same_v<decltype(a2), B<int>::A<const char *, int *>>);

  BA ba1("");
  static_assert(is_same_v<decltype(ba1), B<char>::C<const char *>>);
  static_assert(is_same_v<decltype(ba1), BA<const char *>>);

  BA ba2("", (int *) 0);
  static_assert(is_same_v<decltype(ba2), B<char>::C<const char *, int *>>);
  static_assert(is_same_v<decltype(ba2), BA<const char *, int *>>);
}

namespace non_deduced_args_in_return
{
  template<typename U, typename V>
  struct C
  {
    C(...);
  };

  template<typename U, typename V>
  C(U, V) -> C<U, U>;

  template<typename T>
  using A = C<T *, T *>;

  A a("", "");
  static_assert(is_same_v<decltype(a), C<const char *, const char *>>);
  static_assert(is_same_v<decltype(a), A<const char>>);
}

namespace reversed_template_parameters
{
  template <class T, class U> struct C
  {
    C(...);
  };

  template<class U, class T>
  C(T, U) -> C<T, T>;

  template<class V>
  using A = C<V *, V *>;

  A a("", "");
  static_assert(is_same_v<decltype(a), C<const char *, const char *>>);
  static_assert(is_same_v<decltype(a), A<const char>>);
}

namespace non_type_template_parameter
{
  template <class T, class U> struct C
  {
    C(...);
  };

  template<unsigned I, class T>
  C(T, const char (&)[I]) -> C<T, T>;

  template<class V>
  using A = C<V *, V *>;

  A a("", "");
  static_assert(is_same_v<decltype(a), C<const char *, const char *>>);
  static_assert(is_same_v<decltype(a), A<const char>>);
}

namespace simple_alias_to_aggregate
{
  template<class U> struct X { U u; };
  template<class T> using Y = X<T>;

  Y y1{1};
  static_assert(is_same_v<decltype(y1), X<int>>);
  static_assert(is_same_v<decltype(y1), Y<int>>);

  Y y2{""};
  static_assert(is_same_v<decltype(y2), X<const char *>>);
  static_assert(is_same_v<decltype(y2), Y<const char *>>);
}

namespace ptr_alias_to_aggregate
{
  template<class U> struct X { U u; };
  template<class T> using Y = X<T *>;

  Y y1{""};
  static_assert(is_same_v<decltype(y1), X<const char *>>);
  static_assert(is_same_v<decltype(y1), Y<const char>>);
}

namespace alias_to_alias_to_aggregate
{
  template<class U> struct X { U u; };
  template<class T> using Y = X<T>;
  template<class T> using Z = Y<T>;

  Z z1{1};
  static_assert(is_same_v<decltype(z1), X<int>>);
  static_assert(is_same_v<decltype(z1), Y<int>>);
  static_assert(is_same_v<decltype(z1), Z<int>>);

  Z z2{""};
  static_assert(is_same_v<decltype(z2), X<const char *>>);
  static_assert(is_same_v<decltype(z2), Y<const char *>>);
  static_assert(is_same_v<decltype(z2), Z<const char *>>);
}

namespace alias_to_aggregate_with_explicit_guide
{
  template<class U, class V>
  struct X { U u; V v; };

  template<typename U, typename V>
  X(U, V &) -> X<U, V>;

  template<class S, class T> using Y = X<S, T>;

  const int i = 0;

  Y y1{0, i};
  static_assert(is_same_v<decltype(y1), X<int, const int>>);
  static_assert(is_same_v<decltype(y1), Y<int, const int>>);
}

namespace alias_to_aggregate_with_explicit_non_tmpl_guide
{
  template<class U>
  struct X { U u; };

  X(long) -> X<long>;

  template<class T> using Y = X<T>;

  Y y1{0};
  static_assert(is_same_v<decltype(y1), X<long>>);
  static_assert(is_same_v<decltype(y1), Y<long>>);
}

namespace simple_alias_non_tmpl_guide
{
  template<typename T>
  struct C
  {
    C(...);
  };

  C(int) -> C<int *>;

  template<typename T>
  using A = C<T>;

  A a(1);
  static_assert(is_same_v<decltype(a), C<int *>>);
  static_assert(is_same_v<decltype(a), A<int *>>);
}

namespace alias_non_tmpl_guide
{
  template<typename T>
  struct C
  {
    C(...);
  };

  C(int) -> C<int>;
  C(long) -> C<long *>;

  template<typename T>
  using A = C<T *>;

  A a(1);
  static_assert(is_same_v<decltype(a), C<long *>>);
  static_assert(is_same_v<decltype(a), A<long>>);

  C c(1);
  static_assert(is_same_v<decltype(c), C<int>>);
}

namespace simple_alias_conditional_explicit_ctor
{
  template<typename T>
  struct C
  {
    int val = 1;

    constexpr explicit(sizeof(T) != 0) C(T t)
    { }
  };

  template<>
  struct C<void>
  {
    int val = 0;
  };


  struct B
  { };

  struct D : B
  {
    constexpr operator C<void> () const
    { return {}; }
  };


  C(B) -> C<void>;


  template<typename U>
  using A = C<U>;


  constexpr C c1 = D{};
  static_assert(is_same_v<decltype(c1), const C<void>>);
  static_assert(c1.val == 0);

  constexpr A a1 = D{};
  static_assert(is_same_v<decltype(a1), const C<void>>);
  static_assert(a1.val == 0);

  constexpr C c2(D{});
  static_assert(is_same_v<decltype(c2), const C<D>>);
  static_assert(c2.val == 1);

  constexpr A a2(D{});
  static_assert(is_same_v<decltype(a2), const C<D>>);
  static_assert(a2.val == 1);
}

namespace ptr_alias_conditional_explicit_guide
{
  template<typename T>
  struct C
  {
    int val = 1;

    constexpr explicit(sizeof(T) == sizeof(void *)) C(T t)
    { }
  };

  template<>
  struct C<void *>
  {
    int val = 0;

    constexpr C(const char *)
    { }
  };


  C(const volatile char *) -> C<void *>;


  template<typename U>
  using A = C<U *>;


  constexpr C c1 = "";
  static_assert(is_same_v<decltype(c1), const C<void *>>);
  static_assert(c1.val == 0);

  constexpr A a1 = "";
  static_assert(is_same_v<decltype(a1), const C<void *>>);
  static_assert(a1.val == 0);

  constexpr C c2("");
  static_assert(is_same_v<decltype(c2), const C<const char *>>);
  static_assert(c2.val == 1);

  constexpr A a2("");
  static_assert(is_same_v<decltype(a2), const C<const char *>>);
  static_assert(a2.val == 1);
}

namespace ptr_alias_constrained_ctor
{
  template<typename T>
  struct C
  {
    int val;

    constexpr C(T) requires is_same_v<T, int *>
      : val(1)
    { }

    constexpr C(T) requires is_same_v<T, short *>
      : val(2)
    { }
  };

  template<>
  struct C<void *>
  {
    int val = 0;

    constexpr C(const char *)
    { }
  };


  C(const volatile char *) -> C<void *>;


  template<typename U>
  using A = C<U *>;


  constexpr C c1 = "";
  static_assert(is_same_v<decltype(c1), const C<void *>>);
  static_assert(c1.val == 0);

  constexpr A a1 = "";
  static_assert(is_same_v<decltype(a1), const C<void *>>);
  static_assert(a1.val == 0);


  short s;

  constexpr C c2 = &s;
  static_assert(is_same_v<decltype(c2), const C<short *>>);
  static_assert(c2.val == 2);

  constexpr A a2 = &s;
  static_assert(is_same_v<decltype(a2), const C<short *>>);
  static_assert(a2.val == 2);


  int i;

  constexpr C c3 = &i;
  static_assert(is_same_v<decltype(c3), const C<int *>>);
  static_assert(c3.val == 1);

  constexpr A a3 = &i;
  static_assert(is_same_v<decltype(a3), const C<int *>>);
  static_assert(a3.val == 1);
}

namespace ptr_alias_constrained_guide
{
  template<typename T>
  struct C
  {
    int val;

    constexpr C(int *)
      : val(1)
    { }

    constexpr C(short *)
      : val(2)
    { }
  };

  template<typename T> requires is_same_v<T, int *>
  C(T t) -> C<T>;

  template<typename T> requires is_same_v<T, short *>
  C(T t) -> C<T>;


  template<>
  struct C<void *>
  {
    int val = 0;

    constexpr C(const char *)
    { }
  };


  C(const volatile char *) -> C<void *>;


  template<typename U>
  using A = C<U *>;


  constexpr C c1 = "";
  static_assert(is_same_v<decltype(c1), const C<void *>>);
  static_assert(c1.val == 0);

  constexpr A a1 = "";
  static_assert(is_same_v<decltype(a1), const C<void *>>);
  static_assert(a1.val == 0);


  short s;

  constexpr C c2 = &s;
  static_assert(is_same_v<decltype(c2), const C<short *>>);
  static_assert(c2.val == 2);

  constexpr A a2 = &s;
  static_assert(is_same_v<decltype(a2), const C<short *>>);
  static_assert(a2.val == 2);


  int i;

  constexpr C c3 = &i;
  static_assert(is_same_v<decltype(c3), const C<int *>>);
  static_assert(c3.val == 1);

  constexpr A a3 = &i;
  static_assert(is_same_v<decltype(a3), const C<int *>>);
  static_assert(a3.val == 1);
}

namespace simple_default_args
{
  template<typename T = int>
  struct C
  {
    C(int);
  };

  template<typename T = short>
  using A = C<T>;

  C c = 1;
  static_assert(is_same_v<decltype(c), C<>>);
  static_assert(is_same_v<decltype(c), C<int>>);

  A a = 1;
  static_assert(is_same_v<decltype(a), A<>>);
  static_assert(is_same_v<decltype(a), A<short>>);
}

namespace default_args_in_guide
{
  template<typename T>
  struct identity
  { using type = T; };

  template<typename T>
  using identity_t = typename identity<T>::type;

  template<typename T, typename U>
  struct C
  {
    C(...);
  };

  template<typename T, typename U = T *>
  C(T) -> C<T, identity_t<U>>;

  template<typename V, typename W>
  using A = C<V, W>;

  C c = 1;
  static_assert(is_same_v<decltype(c), C<int, int *>>);

  A a = 1;
  static_assert(is_same_v<decltype(a), C<int, int *>>);
  static_assert(is_same_v<decltype(a), A<int, int *>>);
}

namespace default_args_in_guide_reversed
{
  template<typename T>
  struct identity
  { using type = T; };

  template<typename T>
  using identity_t = typename identity<T>::type;

  template<typename T, typename U>
  struct C
  {
    C(...);
  };

  template<typename T, typename U = T *>
  C(T) -> C<T, identity_t<U>>;

  template<typename W, typename Y>
  using A = C<Y, W>;

  C c = 1;
  static_assert(is_same_v<decltype(c), C<int, int *>>);

  A a = 1;
  static_assert(is_same_v<decltype(a), C<int, int *>>);
  static_assert(is_same_v<decltype(a), A<int *, int>>);
}

namespace default_args_in_guide_with_duplicate
{
  template<typename T>
  struct identity
  { using type = T; };

  template<typename T>
  using identity_t = typename identity<T>::type;

  template<typename T, typename U, typename X>
  struct C
  {
    C(...);
  };

  template<typename T, typename U = T *>
  C(T) -> C<T, identity_t<U>, T>;

  template<typename V, typename W>
  using A = C<V, W, V>;

  C c = 1;
  static_assert(is_same_v<decltype(c), C<int, int *, int>>);

  A a = 1;
  static_assert(is_same_v<decltype(a), C<int, int *, int>>);
  static_assert(is_same_v<decltype(a), A<int, int *>>);
}

namespace default_args_in_guide_with_changed_parameter_order
{
  template<typename T>
  struct identity
  { using type = T; };

  template<typename T>
  using identity_t = typename identity<T>::type;

  template<typename T, typename U, typename V>
  struct C
  {
    C(...);
  };

  template<typename Y, typename T, typename U = T *>
  C(T, Y) -> C<T, identity_t<U>, Y>;

  template<typename V, typename W, typename X>
  using A = C<V, W, X>;

  A a(1, "");
  static_assert(is_same_v<decltype(a), C<int, int *, const char *>>);
  static_assert(is_same_v<decltype(a), A<int, int *, const char *>>);
}

namespace default_type_args_in_alias
{
  template<typename T>
  struct identity
  { using type = T; };

  template<typename T>
  using identity_t = typename identity<T>::type;

  template<int I>
  struct V
  { };

  template<typename T>
  struct X
  { };

  template<typename I, typename J> struct C
  {
    C(...);
  };

  template<typename I = V<1>, typename J = V<2>>
  C(X<J>) -> C<identity_t<I>, J>;

  template<typename K = V<3>, typename L = K>
  using A = C<K, L>;

  A a0{X<V<0>>{}};
  static_assert(is_same_v<decltype(a0), C<V<1>, V<0>>>);
  static_assert(is_same_v<decltype(a0), A<V<1>, V<0>>>);

  A a1{X<V<1>>{}};
  static_assert(is_same_v<decltype(a1), C<V<1>, V<1>>>);
  static_assert(is_same_v<decltype(a1), A<V<1>, V<1>>>);

  A a2{2};
  static_assert(is_same_v<decltype(a2), C<V<3>, V<3>>>);
  static_assert(is_same_v<decltype(a2), A<V<3>, V<3>>>);
}

namespace default_non_type_args_in_alias
{
  template<int I>
  struct X
  { };

  template<int I, int J> struct C
  {
    C(...);
  };

  template<int I = 1, int J = 2> C(X<J>) -> C<+I, J>;

  template<int K = 3, int L = K + 1> using A = C<K, L>;

  template<int K = 3, int L = K + 1> using B = C<+K, L>; // non-deducible K
  template<int K = 1, int L = K + 5> using D = C<+K, L>; // non-deducible K

  A a0{X<0>{}};
  static_assert(is_same_v<decltype(a0), C<1, 0>>);
  static_assert(is_same_v<decltype(a0), A<1, 0>>);

  A a1{X<1>{}};
  static_assert(is_same_v<decltype(a1), C<1, 1>>);
  static_assert(is_same_v<decltype(a1), A<1, 1>>);

  B b0{X<0>{}};
  static_assert(is_same_v<decltype(b0), C<3, 4>>);
  static_assert(is_same_v<decltype(b0), A<>>);

  B b1{X<1>{}};
  static_assert(is_same_v<decltype(b1), C<3, 4>>);
  static_assert(is_same_v<decltype(b1), A<>>);

  D d0{X<0>{}};
  static_assert(is_same_v<decltype(d0), C<1, 0>>);
  static_assert(is_same_v<decltype(d0), A<1, 0>>);
}

namespace recursive_default_argument_use
{
  template<int I, int J> struct C
  {
    C(int);
  };

  template<int X = 1, int K = 3, int L = K + 1>
  using A = C<X, L>;

  template<int X = 1, int K = 3, int L = K + 2, int M = 2>
  using B = C<X, L>;

  template<int X = 1, int K = 3, int L = K + 2, int M = L + 3, int N = 3>
  using D = C<X, M>;

  A a0{0};
  static_assert(is_same_v<decltype(a0), C<1, 4>>);
  static_assert(is_same_v<decltype(a0), A<>>);

  B b0{0};
  static_assert(is_same_v<decltype(b0), C<1, 5>>);
  static_assert(is_same_v<decltype(b0), B<>>);

  D d0{0};
  static_assert(is_same_v<decltype(d0), C<1, 8>>);
  static_assert(is_same_v<decltype(d0), D<>>);
}

namespace default_templ_templ_args_in_alias
{
  template<int I>
  struct O
  {
    template<typename U>
    struct C { };
  };

  template<typename> struct V0 { };
  template<typename> struct V1 { };
  template<typename> struct V2 { };

  template<template<typename> class T>
  struct X
  { };

  template<int I, template<typename> class J> struct C
  {
    C(...);
  };

  template<int I = 1, template<typename> class J = V2>
  C(X<J>) -> C<+I, J>;

  template<int K = 3, template<typename> class L = O<K>::template C>
  using A = C<K, L>;

  A a0{X<V0>{}};
  static_assert(is_same_v<decltype(a0), C<1, V0>>);
  static_assert(is_same_v<decltype(a0), A<1, V0>>);

  A a1{X<V1>{}};
  static_assert(is_same_v<decltype(a1), C<1, V1>>);
  static_assert(is_same_v<decltype(a1), A<1, V1>>);

  A a2{2};
  static_assert(is_same_v<decltype(a2), C<3, O<3>::C>>);
  static_assert(is_same_v<decltype(a2), A<>>);
}

namespace default_arg_1
{
  template<int I = 3>
  struct C
  {
    C();
  };

  template<int I = 2>
  C() -> C<+I>;

  template<int I = 1>
  using A = C<I>;

  template<int I = 1>
  using B = C<+I>;

  A a{};
  static_assert(is_same_v<decltype(a), C<2>>);

  B b{};
  static_assert(is_same_v<decltype(b), C<1>>);
}

namespace template_parameter_pack
{
  template <class ... T> struct C
  {
    C(...);
  };

  template<class ... T> C(T ...) -> C<T ...>;

  template<class U> using A1 = C<U>;
  template<class U, class V> using A2 = C<U, V>;
  template<class U, class V, class W> using A3 = C<U, V, W>;

  template<class ... V> using AA = C<V ...>;

  A1 a1(1);
  static_assert(is_same_v<decltype(a1), C<int>>);
  static_assert(is_same_v<decltype(a1), A1<int>>);

  A2 a2(1, 'c');
  static_assert(is_same_v<decltype(a2), C<int, char>>);
  static_assert(is_same_v<decltype(a2), A2<int, char>>);

  A3 a3(1, 'c', 3L);
  static_assert(is_same_v<decltype(a3), C<int, char, long>>);
  static_assert(is_same_v<decltype(a3), A3<int, char, long>>);

  AA aa1(1);
  static_assert(is_same_v<decltype(aa1), C<int>>);
  static_assert(is_same_v<decltype(aa1), AA<int>>);

  AA aa2(1, 'c');
  static_assert(is_same_v<decltype(aa2), C<int, char>>);
  static_assert(is_same_v<decltype(aa2), AA<int, char>>);

  AA aa3(1, 'c', 3L);
  static_assert(is_same_v<decltype(aa3), C<int, char, long>>);
  static_assert(is_same_v<decltype(aa3), AA<int, char, long>>);
}

namespace template_parameter_pack_ptr_type
{
  template<typename ... P>
  struct C
  {
    C(...);
  };

  template<typename ... T>
  C(T ...) -> C<T ...>;

  template<typename ... Q>
  using A = C<Q *...>;

  template<typename ... R>
  using B = A<R *...>;

  A a0;
  static_assert(is_same_v<decltype(a0), C<>>);
  static_assert(is_same_v<decltype(a0), A<>>);

  B b0;
  static_assert(is_same_v<decltype(b0), C<>>);
  static_assert(is_same_v<decltype(b0), A<>>);
  static_assert(is_same_v<decltype(b0), B<>>);

  A a1("");
  static_assert(is_same_v<decltype(a1), C<const char *>>);
  static_assert(is_same_v<decltype(a1), A<const char>>);

  B b1((int **) 0);
  static_assert(is_same_v<decltype(b1), C<int **>>);
  static_assert(is_same_v<decltype(b1), A<int *>>);
  static_assert(is_same_v<decltype(b1), B<int>>);

  A a2("", (int *) 0);
  static_assert(is_same_v<decltype(a2), C<const char *, int *>>);
  static_assert(is_same_v<decltype(a2), A<const char, int>>);

  B b2((int **) 0, (short **) 0);
  static_assert(is_same_v<decltype(b2), C<int **, short **>>);
  static_assert(is_same_v<decltype(b2), A<int *, short *>>);
  static_assert(is_same_v<decltype(b2), B<int, short>>);
}

namespace template_parameter_pack_nta
{
  template<int ... I>
  struct X
  { };

  template<int ... I> struct C
  {
    C(...);
  };

  template<int ... I> C(X<I> ...) -> C<I ...>;

  template<int I1> using A1 = C<I1>;
  template<int I1, int I2> using A2 = C<I1, I2>;
  template<int I1, int I2, int I3> using A3 = C<I1, I2, I3>;

  template<int ... I> using AA = C<I ...>;

  A1 a1(X<1>{});
  static_assert(is_same_v<decltype(a1), C<1>>);
  static_assert(is_same_v<decltype(a1), A1<1>>);

  A2 a2(X<1>{}, X<2>{});
  static_assert(is_same_v<decltype(a2), C<1, 2>>);
  static_assert(is_same_v<decltype(a2), A2<1, 2>>);

  A3 a3(X<1>{}, X<2>{}, X<3>{});
  static_assert(is_same_v<decltype(a3), C<1, 2, 3>>);
  static_assert(is_same_v<decltype(a3), A3<1, 2, 3>>);

  AA aa1(X<1>{});
  static_assert(is_same_v<decltype(aa1), C<1>>);
  static_assert(is_same_v<decltype(aa1), AA<1>>);

  AA aa2(X<1>{}, X<2>{});
  static_assert(is_same_v<decltype(aa2), C<1, 2>>);
  static_assert(is_same_v<decltype(aa2), AA<1, 2>>);

  AA aa3(X<1>{}, X<2>{}, X<3>{});
  static_assert(is_same_v<decltype(aa3), C<1, 2, 3>>);
  static_assert(is_same_v<decltype(aa3), AA<1, 2, 3>>);
}

namespace template_parameter_pack_templ_templ
{
  template<template<class> class ... TT>
  struct X
  { };

  template<template<class> class ... TT> struct C
  {
    C(...);
  };

  template<template<class> class ... TT> C(X<TT> ...) -> C<TT ...>;

  template<template<class> class T1> using A1 = C<T1>;
  template<template<class> class T1, template<class> class T2> using A2 = C<T1, T2>;
  template<template<class> class T1, template<class> class T2, template<class> class T3> using A3 = C<T1, T2, T3>;

  template<template<class> class ... TT> using AA = C<TT ...>;

  template<class> struct Y1 { };
  template<class> struct Y2 { };
  template<class> struct Y3 { };

  A1 a1(X<Y1>{});
  static_assert(is_same_v<decltype(a1), C<Y1>>);
  static_assert(is_same_v<decltype(a1), A1<Y1>>);

  A2 a2(X<Y1>{}, X<Y2>{});
  static_assert(is_same_v<decltype(a2), C<Y1, Y2>>);
  static_assert(is_same_v<decltype(a2), A2<Y1, Y2>>);

  A3 a3(X<Y1>{}, X<Y2>{}, X<Y3>{});
  static_assert(is_same_v<decltype(a3), C<Y1, Y2, Y3>>);
  static_assert(is_same_v<decltype(a3), A3<Y1, Y2, Y3>>);

  AA aa1(X<Y1>{});
  static_assert(is_same_v<decltype(aa1), C<Y1>>);
  static_assert(is_same_v<decltype(aa1), AA<Y1>>);

  AA aa2(X<Y1>{}, X<Y2>{});
  static_assert(is_same_v<decltype(aa2), C<Y1, Y2>>);
  static_assert(is_same_v<decltype(aa2), AA<Y1, Y2>>);

  AA aa3(X<Y1>{}, X<Y2>{}, X<Y3>{});
  static_assert(is_same_v<decltype(aa3), C<Y1, Y2, Y3>>);
  static_assert(is_same_v<decltype(aa3), AA<Y1, Y2, Y3>>);
}

namespace multiple_aliases_for_template_parameter_pack
{
  template <class ... T> struct C
  {
    C(...);
  };

  template<class ... T> C(T ...) -> C<T ...>;

  template<class U> using A1 = C<U>;
  template<class U, class V> using A2 = C<U, V>;
  template<class U, class V, class W> using A3 = C<U, V, W>;

  template<class U> using B1 = A1<U>;
  template<class U, class V> using B2 = A2<V, U>;
  template<class U, class V, class W> using B3 = A3<W, V, U>;

  A1 a1(1);
  static_assert(is_same_v<decltype(a1), C<int>>);
  static_assert(is_same_v<decltype(a1), A1<int>>);

  A2 a2(1, 'b');
  static_assert(is_same_v<decltype(a2), C<int, char>>);
  static_assert(is_same_v<decltype(a2), A2<int, char>>);

  A3 a3(1, 'b', 3L);
  static_assert(is_same_v<decltype(a3), C<int, char, long>>);
  static_assert(is_same_v<decltype(a3), A3<int, char, long>>);

  B1 b1(1);
  static_assert(is_same_v<decltype(b1), C<int>>);
  static_assert(is_same_v<decltype(b1), A1<int>>);
  static_assert(is_same_v<decltype(b1), B1<int>>);

  B2 b2(1, 'b');
  static_assert(is_same_v<decltype(b2), C<int, char>>);
  static_assert(is_same_v<decltype(b2), A2<int, char>>);
  static_assert(is_same_v<decltype(b2), B2<char, int>>);

  B3 b3(1, 'b', 3L);
  static_assert(is_same_v<decltype(b3), C<int, char, long>>);
  static_assert(is_same_v<decltype(b3), A3<int, char, long>>);
  static_assert(is_same_v<decltype(b3), B3<long, char, int>>);
}

namespace pack_with_simple_argument_multiple_levels
{
  template <class ... CT> struct C
  {
    C(...);
  };

  template<class GT, class ... GU>
  C(GT, GU ...) -> C<GT, GU...>;

  template<class AT, class AU, class ... AV>
  using A = C<AT, AU, AV ...>;

  template<class BT, class BU, class BV, class ... BW>
  using B = A<BV, BU, BT, BW ...>; // also swap template parameters


  C c1(1);
  static_assert(is_same_v<decltype(c1), C<int>>);

  C c2(1, 'b');
  static_assert(is_same_v<decltype(c2), C<int, char>>);

  C c3(1, 'b', 3L);
  static_assert(is_same_v<decltype(c3), C<int, char, long>>);

  C c4(1, 'b', 3L, 4u);
  static_assert(is_same_v<decltype(c4), C<int, char, long, unsigned>>);


  A a2(1, 'b');
  static_assert(is_same_v<decltype(a2), C<int, char>>);
  static_assert(is_same_v<decltype(a2), A<int, char>>);

  A a3(1, 'b', 3L);
  static_assert(is_same_v<decltype(a3), C<int, char, long>>);
  static_assert(is_same_v<decltype(a3), A<int, char, long>>);

  A a4(1, 'b', 3L, 4u);
  static_assert(is_same_v<decltype(a4), C<int, char, long, unsigned>>);
  static_assert(is_same_v<decltype(a4), A<int, char, long, unsigned>>);


  B b3(1, 'b', 3L);
  static_assert(is_same_v<decltype(b3), C<int, char, long>>);
  static_assert(is_same_v<decltype(b3), A<int, char, long>>);
  static_assert(is_same_v<decltype(b3), B<long, char, int>>);

  B b4(1, 'b', 3L, 4u);
  static_assert(is_same_v<decltype(b4), C<int, char, long, unsigned>>);
  static_assert(is_same_v<decltype(b4), A<int, char, long, unsigned>>);
  static_assert(is_same_v<decltype(b4), B<long, char, int, unsigned>>);
}

namespace nested_class_with_parameter_pack
{
  template<typename ... TT>
  struct B
  {
    template<typename ... T>
    struct C
    {
      C(...);
    };

    template<typename ... T>
    C(T ...) -> C<T ...>;

    template<typename ... T>
    using A = C<T ...>;
  };

  template<typename ... T>
  using BA = B<short, char>::A<T ...>;

  BA ba0;
  static_assert(is_same_v<decltype(ba0), B<short, char>::C<>>);
  static_assert(is_same_v<decltype(ba0), BA<>>);

  BA ba1(1);
  static_assert(is_same_v<decltype(ba1), B<short, char>::C<int>>);
  static_assert(is_same_v<decltype(ba1), BA<int>>);

  BA ba2(1, 2L);
  static_assert(is_same_v<decltype(ba2), B<short, char>::C<int, long>>);
  static_assert(is_same_v<decltype(ba2), BA<int, long>>);

  B<char, short>::A b_a0;
  static_assert(is_same_v<decltype(b_a0), B<char, short>::C<>>);
  static_assert(is_same_v<decltype(b_a0), B<char, short>::A<>>);

  B<char, short>::A b_a1(1);
  static_assert(is_same_v<decltype(b_a1), B<char, short>::C<int>>);
  static_assert(is_same_v<decltype(b_a1), B<char, short>::A<int>>);

  B<char, short>::A b_a2(1, 'a');
  static_assert(is_same_v<decltype(b_a2), B<char, short>::C<int, char>>);
  static_assert(is_same_v<decltype(b_a2), B<char, short>::A<int, char>>);
}

namespace single_non_pack_alias_for_pack_param_explicit_guide
{
  template<typename T>
  struct X
  { };

  template<typename T>
  struct Y
  { };

  template<typename ... TT>
  struct C
  {
    C(...);
  };

  template<typename ... TT, typename U>
  C(X<TT ...>, Y<U>) -> C<TT ...>;

  template<typename T>
  using A = C<T>;

  A a(X<int>{}, Y<char>{});
  static_assert(is_same_v<decltype(a), C<int>>);
  static_assert(is_same_v<decltype(a), A<int>>);
}

namespace single_non_pack_alias_for_pack_param
{
  template<typename T>
  struct X
  { };

  template<typename T>
  struct Y
  { };

  template<typename ... TT>
  struct C
  {
    template<typename U>
    C(X<TT ...>, Y<U>)
    { }
  };

  template<typename T>
  using A = C<T>;

  C c(X<int>{}, Y<char>{});
  static_assert(is_same_v<decltype(c), C<int>>);

  A a(X<int>{}, Y<char>{});
  static_assert(is_same_v<decltype(a), C<int>>);
  static_assert(is_same_v<decltype(a), A<int>>);
}

namespace multiple_non_pack_alias_for_pack_param
{
  template<typename ... TT>
  struct X
  { };

  template<typename ... TT>
  struct Y
  { };

  template<typename ... TT>
  struct C
  {
    template<typename U>
    C(X<TT ...>, Y<U>)
    { }
  };

  template<typename T1, typename T2>
  using A = C<T1, T2>;

  A a(X<int, long>{}, Y<char>{});
  static_assert(is_same_v<decltype(a), C<int, long>>);
  static_assert(is_same_v<decltype(a), A<int, long>>);
}

namespace additional_pack_in_guide
{
  template<typename ... TT>
  struct X
  { };

  template<typename ... TT>
  struct Y
  { };

  template<typename T>
  struct C
  {
    template<typename ... U>
    C(X<T>, Y<U ...>)
    { }
  };

  template<typename T>
  using A = C<T>;

  A a0(X<int>{}, Y<>{});
  static_assert(is_same_v<decltype(a0), C<int>>);
  static_assert(is_same_v<decltype(a0), A<int>>);

  A a1(X<int>{}, Y<char>{});
  static_assert(is_same_v<decltype(a1), C<int>>);
  static_assert(is_same_v<decltype(a1), A<int>>);

  A a2(X<int>{}, Y<char, short>{});
  static_assert(is_same_v<decltype(a2), C<int>>);
  static_assert(is_same_v<decltype(a2), A<int>>);
}

namespace multiple_packs_in_guide_with_non_pack_alias
{
  template<typename ... TT>
  struct X
  { };

  template<typename ... TT>
  struct Y
  { };

  template<typename ... TT>
  struct C
  {
    template<typename ... UU>
    C(X<TT ...>, Y<UU ...>)
    { }
  };

  template<typename T>
  using A = C<T>;

  A a0(X<int>{}, Y<>{});
  static_assert(is_same_v<decltype(a0), C<int>>);
  static_assert(is_same_v<decltype(a0), A<int>>);

  A a1(X<int>{}, Y<char>{});
  static_assert(is_same_v<decltype(a1), C<int>>);
  static_assert(is_same_v<decltype(a1), A<int>>);

  A a2(X<int>{}, Y<char, short>{});
  static_assert(is_same_v<decltype(a2), C<int>>);
  static_assert(is_same_v<decltype(a2), A<int>>);
}

namespace multiple_packs_in_guide_with_pack_alias
{
  template<typename ... TT>
  struct X
  { };

  template<typename ... TT>
  struct Y
  { };

  template<typename ... TT>
  struct C
  {
    template<typename ... UU>
    C(X<TT ...>, Y<UU ...>)
    { }
  };

  template<typename ... TT>
  using A = C<TT ...>;

  A a00(X<>{}, Y<>{});
  static_assert(is_same_v<decltype(a00), C<>>);
  static_assert(is_same_v<decltype(a00), A<>>);

  A a01(X<>{}, Y<char>{});
  static_assert(is_same_v<decltype(a01), C<>>);
  static_assert(is_same_v<decltype(a01), A<>>);

  A a02(X<>{}, Y<char, short>{});
  static_assert(is_same_v<decltype(a02), C<>>);
  static_assert(is_same_v<decltype(a02), A<>>);

  A a10(X<int>{}, Y<>{});
  static_assert(is_same_v<decltype(a10), C<int>>);
  static_assert(is_same_v<decltype(a10), A<int>>);

  A a11(X<int>{}, Y<char>{});
  static_assert(is_same_v<decltype(a11), C<int>>);
  static_assert(is_same_v<decltype(a11), A<int>>);

  A a12(X<int>{}, Y<char, short>{});
  static_assert(is_same_v<decltype(a12), C<int>>);
  static_assert(is_same_v<decltype(a12), A<int>>);

  A a20(X<int, long>{}, Y<>{});
  static_assert(is_same_v<decltype(a20), C<int, long>>);
  static_assert(is_same_v<decltype(a20), A<int, long>>);

  A a21(X<int, long>{}, Y<char>{});
  static_assert(is_same_v<decltype(a21), C<int, long>>);
  static_assert(is_same_v<decltype(a21), A<int, long>>);

  A a22(X<int, long>{}, Y<char, short>{});
  static_assert(is_same_v<decltype(a22), C<int, long>>);
  static_assert(is_same_v<decltype(a22), A<int, long>>);
}

namespace multiple_packs_in_guide_with_pack_alias_concatenation
{
  template<typename ... TT>
  struct X
  { };

  template<typename ... TT>
  struct Y
  { };

  template<typename ... TT>
  struct C
  {
    C(...);
  };

  template<typename ... TT, typename ... UU>
  C(X<TT ...>, Y<UU ...>) -> C<TT ..., UU ...>;

  template<typename ... TT>
  using A = C<TT ...>;

  A a00(X<>{}, Y<>{});
  static_assert(is_same_v<decltype(a00), C<>>);
  static_assert(is_same_v<decltype(a00), A<>>);

  A a01(X<>{}, Y<char>{});
  static_assert(is_same_v<decltype(a01), C<char>>);
  static_assert(is_same_v<decltype(a01), A<char>>);

  A a02(X<>{}, Y<char, short>{});
  static_assert(is_same_v<decltype(a02), C<char, short>>);
  static_assert(is_same_v<decltype(a02), A<char, short>>);

  A a10(X<int>{}, Y<>{});
  static_assert(is_same_v<decltype(a10), C<int>>);
  static_assert(is_same_v<decltype(a10), A<int>>);

  A a11(X<int>{}, Y<char>{});
  static_assert(is_same_v<decltype(a11), C<int, char>>);
  static_assert(is_same_v<decltype(a11), A<int, char>>);

  A a12(X<int>{}, Y<char, short>{});
  static_assert(is_same_v<decltype(a12), C<int, char, short>>);
  static_assert(is_same_v<decltype(a12), A<int, char, short>>);

  A a20(X<int, long>{}, Y<>{});
  static_assert(is_same_v<decltype(a20), C<int, long>>);
  static_assert(is_same_v<decltype(a20), A<int, long>>);

  A a21(X<int, long>{}, Y<char>{});
  static_assert(is_same_v<decltype(a21), C<int, long, char>>);
  static_assert(is_same_v<decltype(a21), A<int, long, char>>);

  A a22(X<int, long>{}, Y<char, short>{});
  static_assert(is_same_v<decltype(a22), C<int, long, char, short>>);
  static_assert(is_same_v<decltype(a22), A<int, long, char, short>>);
}

namespace guide_with_pack_being_substituted_with_non_pack_and_pack
{
  template<typename ... TT>
  struct X
  { };

  template<typename ... T>
  struct C
  {
    C(...);
  };

  template<typename ...T>
  C(X<T...>) -> C<T...>;

  template<typename T1, typename ... T>
  using A = C<T1, T ...>;

  A a1(X<int>{});
  static_assert(is_same_v<decltype(a1), C<int>>);

  A a2(X<int, long>{});
  static_assert(is_same_v<decltype(a2), C<int, long>>);
}

namespace guide_for_alias_with_fn_type_pack
{
  template<typename ... TT>
  struct C
  {
    C(TT ...);
  };

  template<typename ... TT>
  using A = C<void (*) (TT) ...>;

  void foo(int);
  void bar(char);

  A a1(&foo);
  static_assert(is_same_v<decltype(a1), C<void(*)(int)>>);
  static_assert(is_same_v<decltype(a1), A<int>>);

  A a2(&foo, &bar);
  static_assert(is_same_v<decltype(a2), C<void(*)(int), void(*)(char)>>);
  static_assert(is_same_v<decltype(a2), A<int, char>>);
}

namespace guide_for_alias_with_fn_param_pack_type
{
  template<typename ... TT>
  struct C
  {
    C(TT ...);
  };

  template<typename ... TT>
  using A = C<void (*) (TT ...)>;

  void foo(int);
  void bar(char, short);

  A a1(&foo);
  static_assert(is_same_v<decltype(a1), C<void(*)(int)>>);
  static_assert(is_same_v<decltype(a1), A<int>>);

  A a2(&bar);
  static_assert(is_same_v<decltype(a2), C<void(*)(char, short)>>);
  static_assert(is_same_v<decltype(a2), A<char, short>>);
}

namespace guide_for_alias_with_nested_pack_and_default_arg
{
  template<typename ... UU>
  struct X
  { };

  template<typename ... CC>
  struct C
  {
    C(CC ...);
  };

  template<typename T = int, typename ... TT>
  using A = C<X<TT ...>>;

  A a0(X<>{});
  static_assert(is_same_v<decltype(a0), C<X<>>>);
  static_assert(is_same_v<decltype(a0), A<>>);
}

namespace pack_expansion_in_requires_expr
{
  template<typename ... UU>
  struct X
  { };

  template<typename ... CC>
  struct C
  {
    C(CC ...) requires (sizeof ... (CC) == 1);
  };

  template<typename U = int, typename ... TT>
  using A = C<X<TT ...>>;

  A a0(X<>{});
  static_assert(is_same_v<decltype(a0), C<X<>>>);

  A a1(X<int>{});
  static_assert(is_same_v<decltype(a1), C<X<int>>>);

  A a2(X<int, char>{});
  static_assert(is_same_v<decltype(a2), C<X<int, char>>>);
}

namespace trailing_requires_clause
{
  template<typename T>
  struct C
  {
    C(...);

    C(T) requires (sizeof(T) == sizeof(int));
  };

  C(long) -> C<void *>;

  template<typename T>
  using A = C<T>;

  A ai(0);
  static_assert(is_same_v<decltype(ai), C<int>>);

  A ac('a');
  static_assert(is_same_v<decltype(ac), C<void *>>);
}

namespace template_head_requires_clause
{
  template<typename T>
  struct C
  {
    C(...);
  };

  template<typename U> requires (sizeof(U) == sizeof(int))
  C(U) -> C<U>;

  C(long) -> C<void *>;

  template<typename T>
  using A = C<T>;

  A ai(0);
  static_assert(is_same_v<decltype(ai), C<int>>);

  A ac('a');
  static_assert(is_same_v<decltype(ac), C<void *>>);
}

namespace sfinae_implicit_deduction_guides
{
  template<typename T>
  struct C;

  template<typename T>
  auto foo(T t, int) -> decltype(C{t});
  template<typename T>
  int foo(T t, int *);

  int i = foo(1, 0);

  template<typename T>
  struct C
  {
    C(T);
  };

  C c{1};
  static_assert(is_same_v<decltype(c), C<int>>);
  C cc{c};
  static_assert(is_same_v<decltype(cc), C<int>>);
}

namespace sfinae_alias_implicit_deduction_guides
{
  template<typename T>
  struct C;

  template<typename U>
  using A = C<U>;

  template<typename T>
  auto foo(T t, int) -> decltype(A{t});
  template<typename T>
  int foo(T t, int *);

  int i = foo(1, 0);

  template<typename T>
  struct C
  {
    C(T);
  };

  A a{1};
  static_assert(is_same_v<decltype(a), C<int>>);
  A aa{a};
  static_assert(is_same_v<decltype(aa), C<int>>);
}

namespace regression
{
  template<typename T>
  struct X
  { };

  template<typename ... T>
  struct C
  { };

  template<typename ... TT>
  struct B
  {
    B(C<X<TT> ...>);
  };

  template<typename ... TT>
  using A = B<TT ...>;

  B b(C<X<int>, X<char>>{});
  static_assert(is_same_v<decltype(b), B<int, char>>);

  A a(C<X<int>, X<char>>{});
  static_assert(is_same_v<decltype(a), B<int, char>>);
  static_assert(is_same_v<decltype(a), A<int, char>>);
}

namespace cwg_2467
{
  template<typename T>
  struct vector
  {
    vector(T, T, T);
  };

  template<typename T = int> using X = vector<int>;
  X x = {1, 2, 3};
  static_assert(is_same_v<decltype(x), vector<int>>);
  static_assert(is_same_v<decltype(x), X<>>);
  static_assert(is_same_v<decltype(x), X<int>>);

  template<typename...> using Y = vector<int>;
  Y y = {1, 2, 3};
  static_assert(is_same_v<decltype(y), vector<int>>);
  static_assert(is_same_v<decltype(y), Y<>>);
}


namespace class_ctad
{
  namespace nested_type_param
  {
    template<typename T>
    struct X
    { };

    template<typename U>
    struct B
    {
      template<typename ... TT> struct C
      {
        C(X<TT> ...);
      };
    };

    B<int>::C c{X<int>{}, X<long>{}};

    static_assert(is_same_v<decltype(c), B<int>::C<int, long>>);
  }

  namespace nested_nttp
  {
    template<int I>
    struct X
    { };

    template<typename U>
    struct B
    {
      template<int ... II> struct C
      {
        C(X<II> ...);
      };
    };

    B<int>::C c{X<1>{}, X<2>{}};

    static_assert(is_same_v<decltype(c), B<int>::C<1, 2>>);
  }

  namespace nested_tmpl
  {
    template<template<class> class T>
    struct X
    { };

    template<typename U>
    struct B
    {
      template<template<class> class ... TT> struct C
      {
        C(X<TT> ...);
      };
    };

    template<typename>
    struct Y1
    { };

    template<typename>
    struct Y2
    { };

    B<int>::C c{X<Y1>{}, X<Y2>{}};

    static_assert(is_same_v<decltype(c), B<int>::C<Y1, Y2>>);
  }

  namespace nested_with_pack
  {
    template<typename T>
    struct X
    { };

    template<typename U>
    struct B
    {
      template<typename ... TT> struct C
      {
        C(X<TT> ...);
      };
    };

    B<int>::C c{X<int>{}, X<long>{}};
    static_assert(is_same_v<decltype(c), B<int>::C<int, long>>);
  }
}
