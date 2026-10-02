//type:fp
//options:--c++20 --gn 150200:--ms_c++20 --microsoft_version 1950:--c++14 --defer_parse_function_templates:--c++14:--c++20
//options_all:-w

namespace minimal
{
  template<typename> struct D1;
  template<typename> struct D2;
  template<typename T> struct B { using type = int; };
  template<typename T> using A = B<D2<T>>;
  template<typename T> struct C { using type = typename T::type; };
  template<typename T> struct C<D1<T>> : C<A<T>> {};
  template<typename T> struct S1 {
    using type = typename C<D1<T>>::type;
    friend int g(S1);
  };
  template<typename T> struct S2 {
    friend int f(S2) {
      return [] (auto) { return g(S1<T>{}); } (1);
    }
  };
  int i = f(S2<int>{});
}

namespace minimal_nested
{
  template<typename> struct D1;
  template<typename> struct D2;
  template<typename T> struct B { using type = int; };
  template<typename T> using A = B<D2<T>>;
  template<typename T> struct C { using type = typename T::type; };
  template<typename T> struct C<D1<T>> : C<A<T>> {};
  template<typename T> struct S1 {
    using type = typename C<D1<T>>::type;
    friend int g(S1);
  };
  template<typename T> struct S2 {
    friend int f(S2) {
      return [] (auto) {
        return [] (auto) { return g(S1<T>{}); }(1);
      }(1);
    }
  };
  int i = f(S2<int>{});
}

#if __cpp_constexpr >= 201603L
namespace class_non_template_fn
{
  struct B { };
  struct D : B { };

  constexpr int g(B) { return 1; }
  constexpr int g(char) { return 1; }

  template<typename T>
  struct C {
    static constexpr int f(T t)
    {
      return [=] (auto p) -> int {
        return g(t) +
               10*g(p);
      } (t) + 100*g(t);
    }
  };

  constexpr int g(D) { return 2; }
  constexpr int g(int) { return 2; }

  static_assert(C<int>::f(1) == 111, "");
}

namespace class_template_fn
{
  struct B { };
  struct D : B { };

  constexpr int g(B) { return 1; }
  constexpr int g(char) { return 1; }

  template<typename U>
  struct C {
    template<typename T>
    static constexpr int f(T t)
    {
      return [=] (auto p) -> int {
        return g(t) +
               10*g(p);
      } (t) + 100*g(t);
    }
  };

  constexpr int g(D) { return 2; }
  constexpr int g(int) { return 2; }

  static_assert(C<int>::f(1) == 111, "");
}

namespace class_friend
{
  struct B { };
  struct D : B { };

  constexpr int g(B) { return 1; }
  constexpr int g(char) { return 1; }

  template<typename T>
  struct C {
    friend constexpr int f(C, T t) {
      return [=] (auto p) -> int {
        return g(t) +
               10*g(p);
      } (t) + 100*g(t);
    }
  };

  constexpr int g(D) { return 2; }
  constexpr int g(int) { return 2; }

  static_assert(f(C<int>{}, 1) == 111, "");
}
#endif

/*
The generic lambda inside f(S2) fails to resolve g(S1<T>{}) via ADL.  Reason:
under deferred prototype instantiation, candidate_function_is_visible also
applies its decl_seq "later declarations" filter to ADL-found candidates, and
the friend g(S1<int>) injected by the sibling S1<int> instantiation carries a
decl_seq larger than the lambda's own, so it is wrongly dropped.  Fix: bypass
that filter for namespace- projection ADL candidates when the call site is a
generic-lambda call op whose lambda is inside a friend definition of a class
template being instantiated (see overload.c / scope_stk.c).
*/
