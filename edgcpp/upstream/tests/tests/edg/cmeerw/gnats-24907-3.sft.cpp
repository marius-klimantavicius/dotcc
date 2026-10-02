//type:fp
//options:--c++20:--ms_c++20 --microsoft_version 1936

// also needs changes for EDGcpfe/27231

namespace parameter_pack_expansion
{
  template<typename T>
  T g(T);

  template<typename ... T>
  void f(T ...);

  void f2(int, int);

  template<typename T>
  struct C
  {
    template<typename ... V>
    void operator () (V ... v) const
    requires requires {
      f(g<V>(v) ...);
      }
    { }

    template<typename ... V>
    void h2 (V ... v) const
    requires requires {
      f2(g<V>(v) ...);
      }
    { }
  };

  void f(C<int> c)
  {
    c();
    c(1);
    c(1, 2);
    c(1, 2, 3);

    c.h2(1, 2);
  }
}

namespace nested_parameter_pack_expansion
{
  void f2(int, int);
  void f4(int, int, int, int);

  template<typename ... T>
  struct C {
    template<typename ... V>
    void h2(V ... v) requires requires (V ... u) { f2(u ...); };

    template<typename ... V>
    void h4(V ... v) requires requires (V ... u, T ... t) { f4(u ..., t ...); };
  };

  void f(C<int, int> c)
  {
    c.h2(1, 2);
    c.h4(1, 2);
  }
}

namespace nested_parameter_pack_expansion_with_fn
{
  template<typename T>
  T g(T);

  void f2(int, int);
  void f4(int, int, int, int);

  template<typename ... T>
  struct C {
    template<typename ... V>
    void h2(V ... v) requires requires (V ... u) { f2(g<V>(u) ...); };

    template<typename ... V>
    void h4(V ... v) requires requires (V ... u, T ... t) { f4(g<V>(u) ..., g<T>(t) ...); };

    template<typename ... V>
    struct N
    {
      void h2(V ... v) requires requires (V ... u) { f2(g<V>(u) ...); };
      void h4(V ... v) requires requires (V ... u, T ... t) { f4(g<V>(u) ..., g<T>(t) ...); };
    };
  };

  void f(C<int, int> c, C<int, int>::N<int, int> n)
  {
    c.h2(1, 2);
    c.h4(1, 2);

    n.h2(1, 2);
    n.h4(1, 2);
  }
}
