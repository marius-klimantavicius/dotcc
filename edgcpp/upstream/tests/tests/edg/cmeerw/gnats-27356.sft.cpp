//type:fp
//options_all:-tused
//options:--c++11 -A:--c++20 -A:--c++11 --clang_version 180100:--c++11 --gn 140100:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  namespace ns {
    template<typename T>
    struct C {};
  }
  template<typename T>
  void f(ns::C<T> c) {
    c.~C<T>();
  }
}

namespace templ_arg_and_no_args
{
  namespace ns
  {
    template<typename T>
    struct C {};
  }

  void f(ns::C<int> c, ns::C<int> *p)
  {
    c.~C();
    c.~C<int>();

    c.ns::C<int>::~C();
    c.ns::C<int>::~C<int>();

    p->~C();
    p->~C<int>();

    p->ns::C<int>::~C();
    p->ns::C<int>::~C<int>();
  }

  template<typename T>
  void f(ns::C<T> c, ns::C<T> *p)
  {
    c.~C();
    c.~C<T>();

#if !defined(__clang__)
    c.ns::C<T>::~C();
    c.ns::C<T>::~C<T>();
#endif

    p->~C();
    p->~C<T>();

#if !defined(__clang__)
    p->ns::C<T>::~C();
    p->ns::C<T>::~C<T>();
#endif
  }

  template void f(ns::C<int>, ns::C<int> *);
}

namespace non_templ
{
  struct C
  {
    ~C();
  };

  void f(C c)
  {
    c.~C();
  }
}

namespace templ_param_type
{
  template<typename T>
  T f();

  template<typename T>
  struct C { T t; };

  template<typename T, typename = decltype(f<C<T>>().~C<T>())>
  void g();

  template<typename T>
  void g(T t)
  {
    t.~C<T>();
  }
}
