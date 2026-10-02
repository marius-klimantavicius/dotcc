//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename ...> concept C = true;
  struct A {
    template<typename ... T> requires (C<T> && ...)
    friend class B;
  };
  template<typename ... T> requires (C<T> && ...)
  class B;
}

namespace non_tmpl_class_scope
{
  template<class> concept C = true;

  struct X
  {
    template<class... T> requires (C<T> && ...)
    friend class F;

  private:
    static constexpr int value = 0;
  };

  template<class... T> requires (C<T> && ...)
  struct F
  {
    static constexpr int value = X::value;
  };

  F<char, short, int> f;
}

namespace non_tmpl_class_scope_fn
{
  template<class> concept C = true;

  template<typename ... T>
  constexpr bool fn(T ... t)
  {
    return (t && ...);
  }

  struct X
  {
    template<class... T> requires (fn(C<T> ...))
    friend class F;

  private:
    static constexpr int value = 0;
  };

  template<class... T> requires (fn(C<T> ...))
  struct F
  {
    static constexpr int value = X::value;
  };

  F<char, short, int> f;
}

namespace tmpl_class_scope
{
  template<class> concept C = true;

  template<typename U>
  struct X
  {
    template<class... T> requires (C<T> && ...)
    friend class F;

  private:
    static constexpr int value = 0;
  };

  X<void> x;

  template<class... T> requires (C<T> && ...)
  struct F
  {
    static constexpr int value = X<void>::value;
  };

  F<char, short, int> f;
}

namespace tmpl_class_scope_fn
{
  template<class> concept C = true;

  template<typename ... T>
  constexpr bool fn(T ... t)
  {
    return (t && ...);
  }

  template<typename U>
  struct X
  {
    template<class... T> requires (fn(C<T> ...))
    friend class F;

  private:
    static constexpr int value = 0;
  };

  X<void> x;

  template<class... T> requires (fn(C<T> ...))
  struct F
  {
    static constexpr int value = X<void>::value;
  };

  F<char, short, int> f;
}

namespace sizeof_pack_expansion
{
  template<int I>
  struct C
  { };

  template<typename U>
  struct X
  {
    template<int I, class... T> requires (sizeof...(T) == I)
    friend int f(X, C<I>, T ...);
  };

  struct Y
  {
    template<int I, class... T> requires (sizeof...(T) == I)
    friend int g(Y, C<I>, T ...);
  };

  int i1 = f(X<int>(), C<1>(), 1);
  int i2 = f(X<int>(), C<2>(), 1, 2);

  int j1 = g(Y(), C<1>(), 1);
  int j2 = g(Y(), C<2>(), 1, 2);
}

namespace ctad_explicit_bool_sizeof_pack
{
  template<class ...T>
  struct A
  {
    explicit(sizeof ... (T)) A() { }
  };

  A a;


  template<class ...T>
  struct B
  {
    explicit(sizeof ... (T)) B(T ...) { }
  };

  B b;
  B b1(1);
  B b2(1, 2);
}
