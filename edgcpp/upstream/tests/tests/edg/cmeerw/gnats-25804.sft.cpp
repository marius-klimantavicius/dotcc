//type:fp
//options:--c++11:--c++20:--ms_c++20

namespace minimal
{
  struct B {
    int f();
  };
  template<typename T>
  struct D : private T {
    using T::f;
  };
  template<typename T>
  auto g(D<T> d) -> decltype(d.f());
  int i = g(D<B>());
}

namespace private_base_class
{
  template<typename T>
  auto f(T t) -> decltype(t.operator()(1))
  { return { }; }

  template<typename T>
  class O : T
  {
  public:
    using T::operator();
  };

  struct C
  {
    int operator () (int);
  };

  int i = f(O<C>());
}

namespace protected_member
{
  template<typename T>
  auto f(T t) -> decltype(t.g(1))
  { return { }; }

  template<typename T>
  struct O : T
  {
    using T::g;
  };

  struct C
  {
  protected:
    int g(int);
  };

  int i = f(O<C>());
}

namespace data_member_in_private_base_class
{
  template<typename T>
  auto f(T t) -> decltype(t.m)
  { return { }; }

  template<typename T>
  class O : T
  {
  public:
    using T::m;
  };

  struct C
  {
    int m;
  };

  int i = f(O<C>());
}

namespace protected_data_member
{
  template<typename T>
  auto f(T t) -> decltype(t.m)
  { return { }; }

  template<typename T>
  struct O : T
  {
    using T::m;
  };

  struct C
  {
  protected:
    int m;
  };

  int i = f(O<C>());
}

namespace private_base_class_with_tmpl
{
  template<typename T>
  auto f(T t) -> decltype(t.template g<int>(1))
  { return { }; }

  template<typename T>
  class O : T
  {
  public:
    using T::g;
  };

  struct C
  {
    template<typename U>
    U g(U);
  };

  int i = f(O<C>());
}

namespace protected_tmpl_member
{
  template<typename T>
  auto f(T t) -> decltype(t.template g<int>(1))
  { return { }; }

  template<typename T>
  struct O : T
  {
    using T::g;
  };

  struct C
  {
  protected:
    template<typename U>
    U g(U);
  };

  int i = f(O<C>());
}

namespace inaccessbible
{
  struct B
  {
    void f();
  };

  template<typename T>
  struct D : T
  {
  private:
    using T::f;
  };

  template<typename T>
  auto g(D<T> d) -> decltype(d.f());

  int g(B);

  int i = g(D<B>());
}
