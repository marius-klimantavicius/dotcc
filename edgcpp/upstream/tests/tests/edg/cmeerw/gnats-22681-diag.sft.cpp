//type:fn
//options:--c++20:--ms_c++20 --microsoft_version=1927

template<typename T1, typename T2>
constexpr bool is_same_v = false;

template<typename T>
constexpr bool is_same_v<T, T> = true;


namespace alias_deduction_guide
{
  template<typename T>
  using A = int;

  A(int) -> A<int>;             // deduction guide for alias template
}

namespace dependent_nested_name_typename
{
  template<typename T>
  using A = typename T::type;   // non-deducible template

  A a{1};                       // error
}

namespace dependent_nested_name_class_template
{
  template<typename T>
  struct B
  {
    template<typename U>
    struct C
    {
      C(U);
    };
  };

  template<typename T, typename U>
  using A = B<T>::template C<U>; // non-deducible template

  template<typename T, typename U>
  using AA = A<T, U>;

  A a1{1};                      // error
  AA a2{2};                     // error
}

namespace is_deducible_constraint
{
  template<typename T> struct identity { using type = T; };
  template<typename T> using identity_t = typename identity<T>::type;

  template <class T, class U> struct C {
    C(...);
  };

  template<class T, class U>
  C(T, U) -> C<T, identity_t<U>>;

  template<class V>
  using A = C<V *, V *>;

  A a1("", "");
  static_assert(is_same_v<decltype(a1), A<const char>>);
  static_assert(is_same_v<decltype(a1), C<const char *, const char *>>);

  A a2((char *) 0, (int *) 0);  // cannot deduce A<V> from C<char *, int *>
}

namespace is_deducible_unused_parameter
{
  template <class T> struct C {
    C(...);
  };

  template<class T>
  C(T) -> C<T>;

  template<class V, class W>
  using A = C<V>;

  A a = 1;                      // cannot deduce W in A
}

namespace non_dependent_is_deducible
{
  template<typename T>
  struct C
  { C(...); };

  C(int) -> C<int *>;

  template<typename T>
  using A = C<T &>;

  A a(1);                       // cannot deduce T in A
}

namespace failed_substitution
{
  template<typename T>
  struct C
  {
    C(int);
  };

  template<typename T> requires T::value
  C(T) -> C<T *>;

  template<typename T>
  using A = C<T *>;

  struct B
  {
    static constexpr bool value = true;

    operator int () const;
  };

  A ab(B{});
  static_assert(is_same_v<decltype(ab), C<B *>>);
  static_assert(is_same_v<decltype(ab), A<B>>);

  A a1(1);                      // cannot deduce arguments for A
}

namespace failed_return_type_deduction
{
  template <class S1, class S2>
  struct C { C(...); };

  template<class T1> C(T1) -> C<T1, T1>;
  template<class T1, class T2> C(T1, T2) -> C<T1 *, T2>;

  template<class V1, class V2> using A = C<V1, V2>;

  C c1{""};                     // OK
  A a1{""};                     // cannot deduce C<T1, T1> from C<V1, V2>
  C c2{"", 1};                  // OK
  A a2{"", 1};                  // cannot deduce C<T1 *, T2> from C<V1, V2>
}

// same as g++/cpp2a/class-deduction-alias3.C (using is_same_v instead of __is_same)
namespace gpp_class_deduction_alias3
{
  template<class T, class U>
  struct X { X(U) requires is_same_v<U, int> {} };

  template<class U>
  using Y = X<void, U>;

  Y y{1};
  Y z{'a'}; // { dg-error "failed|no match" }
}

// same as g++/cpp2a/class-deduction-alias8.C (using is_same_v instead of __is_same)
namespace gpp_class_deduction_alias8
{
  template<class T, class U>
  struct X { X(U) requires is_same_v<U, int> {} };

  template<class U>
  X(U) -> X<char, U>;

  template<class U>
  using Y = X<void, U>;

  Y y{1};
  Y z{'a'}; // { dg-error "failed|no match" }
}

namespace invalid_is_deducible
{
  static_assert(__edg_is_deducible(int, int)); // expect an identifier

  using INT = int;
  static_assert(__edg_is_deducible(INT, int)); // not a class template

  int i;
  static_assert(__edg_is_deducible(i, int)); // not a class template
}

namespace alias_to_aggregate_with_explicit_guide
{
  template<class U, class V> struct X { U u; V v; };

  template<typename U, typename V>
  X(U, V &) -> X<U, V>;

  template<class S, class T> using Y = X<S, T>;

  Y y1{0, 1};                   // cannot deduce template arguments
}

namespace guide_for_incomplete_type
{
  template<typename T = int>
  struct C;

  template<typename U = long>
  using A = C<U>;

  C c1{};                       // incomplete type (but deduction succeeds)
  A a1{};                       // incomplete type (but deduction succeeds)

  template<typename V = char>
  using B = A<V>;

  B b1{};                       // incomplete type (but deduction succeeds)

  template<typename T>
  struct C
  {
    C(T);
  };

  C c2{};                       // deduction fails
  A a2{};                       // deduction fails
  B b2{};                       // deduction fails

  C c3{3};
  static_assert(is_same_v<decltype(c3), C<int>>);

  A a3{3};
  static_assert(is_same_v<decltype(a3), A<int>>);

  B b3{3};
  static_assert(is_same_v<decltype(b3), B<int>>);

  C cc{c3};
  static_assert(is_same_v<decltype(cc), C<int>>);

  A aa{a3};
  static_assert(is_same_v<decltype(aa), A<int>>);

  B bb{b3};
  static_assert(is_same_v<decltype(bb), B<int>>);
}

namespace incomplete_type_with_explicit_guides
{
  template<typename T = int>
  struct C;

  C(int) -> C<void>;

  C c0{};                       // incomplete type (but deduction succeeds)
  C c1{1};                      // incomplete type (but deduction succeeds)

  template<typename T>
  struct C
  {
    C(int);
    C(T *);
  };

  C c2{2};
  static_assert(is_same_v<decltype(c2), C<void>>);
}
