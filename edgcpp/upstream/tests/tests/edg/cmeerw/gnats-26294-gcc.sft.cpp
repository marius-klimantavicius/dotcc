//type:fn
//options:--c++03 --gn 150100:--c++03 --clang_version 210100

namespace unnamed_enum
{
  template<typename R, typename A1> void f0(R (*)(A1));
  template<typename T> void f1(T);
  template<typename T, typename U> void f2(T, U);

  enum { E1 };

  void f()
  {
    f0( &f1<__typeof__(E1)>);

    void (*fp)(int, __typeof__(E1)) = f2;
  }
}

namespace unnamed_class
{
  template<typename R, typename A1> void f0(R (*)(A1));
  template<typename T> void f1(T);
  template<typename T, typename U> void f2(T, U);

  struct { } x;

  void f() {
    f0( &f1<__typeof__(x)>);
    void (*fp)(int, __typeof__(x)) = f2;
  }
}

namespace typedef_unnamed_class
{
  template<typename R, typename A1> void f0(R (*)(A1));
  template<typename T> void f1(T);
  template<typename T, typename U> void f2(T, U);

  struct { } x;
  typedef __typeof__(x) X;

  void f() {
    f0( &f1<X>);
    void (*fp)(int, X) = f2;
  }
}
