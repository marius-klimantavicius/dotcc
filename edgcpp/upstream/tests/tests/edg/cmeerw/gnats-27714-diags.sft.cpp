//type:fn
//options:--c++20

namespace repeated_not_satisfied
{
  template<typename T>
  concept X = false;

  template<typename T> requires X<T>
  struct B1
  { };

  template<typename T> requires (!!X<T>)
  struct B2
  { };

  B1<int> b1a;                  // error
  B1<int> b1b;                  // error

  B2<int> b2a;                  // error
  B2<int> b2b;                  // error
}

namespace depends_on_itself
{
  template<typename T>
  struct A
  { };

  template<typename T>
  concept C = requires { A<T *>(); }; // error: depends on itself

  template<typename T> requires C<T>
  struct A<T *>
  { };

  template<typename T> requires C<T>
  struct B
  { };

  B<int> b1;
  B<int> b2;
}

namespace not_boolean
{
  template<typename T> requires (sizeof(T)) // error: must have type bool
  struct C
  { };

  C<int> c1;                    // error
  C<int> c2;                    // error
}

namespace invalid_constraint
{
  template<typename T> requires INVALID
  struct C
  { };

  C<int> c1;                    // error
  C<int> c2;                    // error
}

namespace eval_failure
{
  template<typename T>
  constexpr bool f(T);

  template<typename T> concept C1 = f(true);
  template<typename T> concept C2 = C1<T>;

  template<typename T>
  constexpr bool f(T)
  {
    return C2<T>;
  }

  template<typename T> requires C2<T> int g(T);

  int i = g(1);                 // error
}

namespace non_constant_eval
{
  int g;

  template<typename T>
  constexpr bool f()
  { return g; }                 // not constant

  template<typename T>
  struct B {
    template<int> static int g() requires (f<T>());
  };

  int i = B<int>::g<0>() + B<int>::g<1>(); // error
}

namespace failed_disjunct
{
  constexpr bool f() {return false;}
  constexpr bool g() {return false;}

  constexpr bool fv = false;
  constexpr bool gv = false;

  template <class T> requires (f() || g()) struct A {};
  A<short> a;                   // error

  template <class T> requires (fv || gv) struct B {};
  B<short> b;                   // error
}
