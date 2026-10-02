//type:fp
//options:--c++14:--c++20:--ms_c++20

namespace minimal
{
  struct C {
    int operator () (int = 0);
  };
  template<typename T> C c;
  template<typename T> using F = T;
  template<typename T, F<decltype(c<T *>(1))>> void f(T) { }
  template<typename T, F<decltype(c<T &>(1))>> void f(T) { }
  template<typename T, F<decltype(c<T>())>> void g(T) { }
  template<typename T, F<decltype(c<T>())>> void h(T) { }
}

namespace distinct_types
{
  template<typename T> using F = T;

  struct C0
  {
    int operator () ();
  };

  struct C1
  {
    int operator () (int);
  };

  namespace
  {
    template<typename T, typename U = void> C0 c0;
    template<typename T> C1 c1;
    template<typename T> int d;

    template<typename T> C1 s1;
    template<> C1 s1<char>;
    template<> C1 s1<int>;
  }

  template<typename T, F<decltype(c0<T>())>> void f() { }
  template<typename T, F<decltype(c0<T, int>())>> void f() { }
  template<typename T, F<decltype(c0<T, long>())>> void f() { }
  template<typename T, F<decltype(c0<T *>())>> void f() { }

  template<typename T, F<decltype(s1<char>(T(1)))>> void f() { }
  template<typename T, F<decltype(s1<int>(T(1)))>> void f() { }

  template<typename T, F<decltype(c1<T>(T(1)))>> void f() { }
  template<typename T, F<decltype(c1<T>(T(2)))>> void f() { }
  template<typename T, F<decltype(c1<T *>(T(1)))>> void f() { }

  template<typename T, F<decltype(d<T>)>> void f() { }
  template<typename T, F<decltype(d<T *>)>> void f() { }
}
