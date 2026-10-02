//type:fp
//options:--c++11:--c++14:--c++20:--c++20 --gn 140100:--c++20 --clang_version 180100:--ms_c++20 --microsoft_version 1936

namespace minimal
{
  template<typename T> T g();
  template<typename T> struct A {
    int m;
    int f() {
      return g<A &>().A::m = 1;
    }
  };
  int i = A<int>().f();
}

namespace tmpl
{
  template<typename T>
  T &g(T&);

  template<typename T>
  T h();

  template<typename T>
  struct A
  {
    using ref = A &;

    int m;

    A &r;
    A *p;

    template<typename>
    void tf() &;

    int f()
    {
      r.tf<int>();
      r.A::tf<int>();
      r.m = 1;
      r.A::m = 1;

#if !defined(__clang__)
      (*p).tf<int>();
      (*p).A::tf<int>();
#endif
      (*p).m = 1;
      (*p).A::m = 1;

      p->tf<int>();
      p->A::tf<int>();
      p->m = 1;
      p->A::m = 1;

      g(*this).template tf<int>();
#if !defined(__clang__)
      g(*this).A::tf<int>();
#endif
      g(*this).m = 1;
      g(*this).A::m = 1;

      h<A &>().m = 1;
      h<A &>().A::m = 1;

      h<typename A::ref>().m = 1;
      h<typename A::ref>().A::m = 1;

      return 0;
    }
  };

  void f(A<int> a)
  {
    a.f();
  }
}
