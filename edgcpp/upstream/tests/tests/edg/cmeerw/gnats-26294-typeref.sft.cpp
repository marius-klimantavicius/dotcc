//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1944:--c++20 --gn 150100:--c++20 --clang_version 210100

namespace std {
  typedef decltype(sizeof 0) size_t;

  template<typename _E> struct initializer_list {
    typedef _E value_type;
    typedef _E const &reference;
    typedef _E const &const_reference;
    typedef _E const *iterator;
    typedef _E const *const_iterator;
    typedef std::size_t size_type;

    constexpr initializer_list(): __array(0), __length((size_t)0) {}

    constexpr size_type size() const { return this->__length; }
    constexpr iterator begin() const { return this->__array; }
    constexpr iterator end() const {
      return this->__length == 0 ? this->__array
             : this->__array + this->__length;
    }
  private:
    iterator __array;
    size_type __length;
    constexpr initializer_list(_E const __a[], std::size_t __l)
      : __array(__a), __length(__l) {}
  };

  template<typename _E> constexpr
  typename initializer_list<_E>::iterator begin(initializer_list<_E> il) {
    return il.begin();
  }

  template<typename _E> constexpr
  typename initializer_list<_E>::iterator end(initializer_list<_E> il) {
    return il.end();
  }
}

namespace aggr_templ_arg_deduction_minimal
{
  struct A {
    int i;
  };

  using AA = A;

  template<typename T>
  struct B {
    AA a;
    T t;
  };

  B b = { 1, 2 };
}

namespace aggr_templ_arg_deduction
{
  template<typename T>
  struct S {
    T x;
    T y;
  };

  using SINT = S<int>;

  template<typename T>
  struct D {
    SINT s;
    T t;
  };

  D d = { 1, 2, 3 };
}

namespace alias_templ_arg_deduction
{
  template<typename T>
  struct G { T t; };

  G g = {1};

  template<typename X = int>
  using BG = G<int>;
  BG bg(1.0);
}

namespace ctad_initializer_list
{
  template<typename T> struct X
  {
    X();
    X(std::initializer_list<T>);
  };

  using XINT = X<int>;

  namespace ns1
  {
    XINT xi;
    X xi2 = { xi };

    using type = decltype(xi);
    using type = decltype(xi2);
  }

  namespace ns2
  {
    X<int> xi;
    X xi2 = { xi };

    using type = decltype(xi);
    using type = decltype(xi2);
  }
}

namespace typedef_more_constrained
{
  template<bool>
  struct A
  { };

  template<bool B1>
  struct D
  { };

  using Dtrue = D<true>;

  template<bool B3>
  void f(Dtrue const &) = delete;

  template<bool B3>
  void f(D<true> const &) requires true;

  template<bool B3>
  void g(Dtrue const &, A<B3>) = delete;

  template<bool B3>
  void g(D<true> const &, A<B3>) requires true;

  void foo(D<true> d, A<true> a1)
  {
    f<true>(d);
    g(d, a1);
  }
}

namespace typedef_more_constrained_minimal
{
  using INT = int;

  template<typename T>
  int f(INT const &) = delete;

  template<typename T>
  int f(int const &) requires true;

  int i = f<int>(0);
}

namespace decltype_typeref_needed
{
  template<typename>
  struct C
  { };

  template<unsigned>
  struct D
  { };

  template<typename T>
  inline int f(T)
  {
    return T::invalid;
  }

  C<decltype(f(1))> *p1;
  D<sizeof(f('a'))> *p2;
}

namespace projection_naming_type
{
  struct B
  {
    using type = int;
  };

  struct D : B
  { };

  using DD = D;

  template<typename T>
  struct C
  {
    template<typename U>
    static D::type t;

    template<typename U>
    static DD::type tt;
  };

  C<int> c;
}

namespace qualified_placeholder
{
  namespace ns
  {
    template<typename T>
    struct C
    { C(T); };
  };

  ns::C c(1);
  auto v = ns::C(1);
  auto p = new ns::C(1);
}

namespace qualified_placeholder_with_guide
{
  namespace ns
  {
    template<typename T>
    struct C
    { C(T); };

    C(int) -> C<int>;
  };

  ns::C c(1);
  auto v = ns::C(1);
  auto p = new ns::C(1);
}

namespace ptr_to_member_decltype
{
  template<typename Z>
  using B = int Z::*;

  template<typename Z>
  using C = decltype(Z());

  template<typename T>
  void f(B<C<T>> arg);
}

namespace ptr_to_member_cv_qual_alias
{
  template<typename T>
  struct C
  {
    template <class U> static char f(void (U::*)());
    template <class U> static int f(...);
  };

  struct B
  { };

  using A = B;

  auto v = C<void>::f<const A>(0);
}
