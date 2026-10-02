//type:fp
//options:--c++20 -tused

namespace local_constant_name_reference
{
  class C {
  public:
    void publicFunc();

  protected:
    void protectedFunc();
  };

  template<typename T, void (C::*p)()>
  struct B
  { };

  template<typename T> struct B<T, &C::publicFunc>
  { };

  template<typename T> struct B<T, &C::protectedFunc>
  { };
}

namespace shared_constant
{
  struct A1 {};
  struct A2 {};

  template<typename T>
  struct C
  {
    static const T t = 1;
  };

  template <class T, class U> void f(T, U)
  {
    static const U I = 0;
    auto x2 = I;
    auto x3 = C<U>::t;
  }

  void g()
  {
    f(A1(), short(0));
    f(A2(), long(0));
  }
}
