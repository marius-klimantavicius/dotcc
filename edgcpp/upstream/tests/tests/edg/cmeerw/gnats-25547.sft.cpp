//type:fp
//options:--c++17:--c++17 --g++:--ms_c++17

namespace fn_default_arg
{
  template<typename> using A = int;
  template<typename> int v;

  void f(int x = v<A<int>>, int y = v<A<int>>) { }
  void g(int x = v<A<int> >, int y = v<A<int> >) { }
  void h(int x = (v<A<int>>), int y = (v<A<int>>)) { }

  void bar()
  {
    f();
    g();
    h();
  }
}

namespace mbr_fn_default_arg
{
  template<typename> using A = int;
  template<typename> int v;

  struct C
  {
    void f(int x = v<A<int>>, int y = v<A<int>>) { }
    void g(int x = v<A<int> >, int y = v<A<int> >) { }
    void h(int x = (v<A<int>>), int y = (v<A<int>>)) { }

    void bar()
    {
      f();
      g();
      h();
    }
  };
}

namespace fn_tmpl_default_arg
{
  template<typename> using A = int;
  template<typename> int v;

  template<typename T = int> void f(int x = v<A<T>>, int y = v<A<T>>) { }
  template<typename T = int> void g(int x = v<A<T> >, int y = v<A<T> >) { }
  template<typename T = int> void h(int x = (v<A<T>>), int y = (v<A<T>>)) { }

  void bar()
  {
    f<>();
    g<>();
    h<>();
  }
}

namespace mbr_fn_tmpl_default_arg
{
  template<typename> using A = int;
  template<typename> int v;

  struct C
  {
    template<typename T = int> void f(int x = v<A<T>>, int y = v<A<T>>) { }
    template<typename T = int> void g(int x = v<A<T> >, int y = v<A<T> >) { }
    template<typename T = int> void h(int x = (v<A<T>>), int y = (v<A<T>>)) { }

    void bar()
    {
      f<>();
      g<>();
      h<>();
    }
  };
}

namespace fn_tmpl_default_tmpl_arg
{
  template <typename> using A = int;
  template <typename> constexpr int v = 1;

  template<typename T = int, int x = v<A<T>>, int y = v<A<T>>>
  void f()
  { }

  template<typename T = int, int x = v<A<T> >, int y = v<A<T> >>
  void g()
  { }

  template<typename T = int, int x = v<A<T> >, int y = v<A<T> > >
  void h()
  { }

  void bar()
  {
    f<>();
    g<>();
    h<>();
  }
}

namespace mbr_fn_tmpl_default_tmpl_arg
{
  template <typename> using A = int;
  template <typename> constexpr int v = 1;

  struct C
  {
    template<typename T = int, int x = v<A<T>>, int y = v<A<T>>>
    void f()
    { }

    template<typename T = int, int x = v<A<T> >, int y = v<A<T> >>
    void g()
    { }

    template<typename T = int, int x = v<A<T> >, int y = v<A<T> > >
    void h()
    { }

    void bar()
    {
      f<>();
      g<>();
      h<>();
    }
  };
}

namespace cls_tmpl_default_arg
{
  template <typename> using A = int;
  template <typename> constexpr int v = 1;

  template<typename T = int, int x = v<A<T>>, int y = v<A<T>>>
  struct F
  { };

  template<typename T = int, int x = v<A<T> >, int y = v<A<T> >>
  struct G
  { };

  template<typename T = int, int x = v<A<T> >, int y = v<A<T> > >
  struct H
  { };

  F<> f;
  G<> g;
  H<> h;
}
