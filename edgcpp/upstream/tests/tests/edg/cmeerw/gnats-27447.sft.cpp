//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1936:--c++20 --gn 130200:--c++20 --clang_version 180100
//options_all:-w

namespace minimal
{
  struct C {
    int i, j;
  };
  C *c = new C(1, 2);
}

namespace minimal_subst
{
  template<typename T> T &&v();
  template<typename T, typename U, typename = decltype(::new T(v<U>()))>
  int *f(int);
  template<typename, typename>
  char *f(...);
  char *p = f<char[2], const char (&)[2]>(0);
}

namespace aggregate_init
{
  template<typename T> T &&v();

  template<typename T, typename U, typename = decltype(::new T(U(1), U(2)))>
  int *f(int);

  template<typename, typename>
  char *f(...);

  struct C
  {
    int i;
    int j;
  };

  int *p = f<C, int>(0);
  int *q = f<int[2], int>(0);
}

namespace inits
{
  struct C
  {
    int i;
    int j;
  };

  void f()
  {
    C c1{ 1, 2 };
    C c2( 2, 3 );

    C *p1 = new C{ 3, 4 };
    C *p2 = new C( 4, 5 );

    using arr_t = int[2];

    arr_t a1{ 5, 6 };
    arr_t a2( 6, 7 );

    int *q1 = new arr_t{ 7, 8 };
    int *q2 = new arr_t( 8, 9 );

    C{ 9, 10 };
    C( 10, 11 );

    arr_t{ 11, 12 };
  }
}

namespace inits_nsdmi
{
  struct C
  {
    int i;
    int j = 1;
  };

  void f()
  {
    C c1{ 1, 2 };
    C c2( 2, 3 );

    C *p1 = new C{ 3, 4 };
    C *p2 = new C( 4, 5 );

    using arr_t = int[2];

    arr_t a1{ 5, 6 };
    arr_t a2( 6, 7 );

    int *q1 = new arr_t{ 7, 8 };
    int *q2 = new arr_t( 8, 9 );

    C{ 9, 10 };
    C( 10, 11 );

    arr_t{ 11, 12 };
  }
}

namespace possible_regression_deduction
{
  template<class T> auto f(T x) -> decltype(new auto(T{}));
  int *p = f<int>('a');
}

namespace possible_regression_conv
{
  struct B;

  struct C
  {
    int i;
    int j;
  };

  struct B
  {
    operator C() const;
  };

  void f(B b)
  {
    C c(b);
    C *p = new C(b);
  }
}

namespace possible_regression_empty_expansion
{
  template<class T, class ... U> auto f() -> decltype(new T(U{} ...));
  auto *p = f<char>();

  template<class T, class ... U> auto g() -> decltype(new T{U{} ...});
  auto *q = g<char>();
}

namespace possible_regression_lifetime
{
  struct X
  {
    ~X();
  };

  struct C
  {
    C(int, const X &);
  };

  C *p = new C(1, X());
}

namespace possible_regression_dpdt_array
{
  template<typename T, typename ... U>
  auto f() -> decltype(new T[](U{} ...));

  int *p = f<int, int, int>();
}
