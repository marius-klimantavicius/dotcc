//type:fp
//options:--c++17:--ms_c++17
namespace gnats
{
  template<typename... T> class C
  {
  public:
    class I {};
  };

  template<typename... Args>
  using F =
      typename C<Args...>::I;

  template<class T, typename... A>
  class R: public C<A...>
  {
  public:
    template<typename... B>
    static F<A...>
    c(B... args)
    {
      return new T(args...);
    }
  };
}

namespace alias_elab
{
  template<typename... T> class C
  {
  public:
    class I {};
  };

  template<typename... Args>
  using F = class C<Args...>::I;

  template<class T, typename... A>
  class R: public C<A...>
  {
  public:
    template<typename... B>
    static F<A...>
    c(B... args)
    {
      return new T(args...);
    }
  };
}

namespace var_tmpl
{
  template<typename T> class C
  {
  public:
    class I {};
  };

  template<typename T>
  using F = typename C<T>::I;

  template<typename T>
  using G = class C<T>::I;

  template<typename T>
  F<T> f;

  template<typename T>
  G<T> g;
}

namespace fn_tmpl
{
  template<typename T> class C
  {
  public:
    class I {};
  };

  template<typename T>
  using F = typename C<T>::I;

  template<typename T>
  using G = class C<T>::I;

  template<typename T>
  F<T> f();

  template<typename T>
  G<T> g();
}

namespace mbr_fn_tmpl
{
  template<typename T> class C
  {
  public:
    class I {};
  };

  template<typename T>
  using F = typename C<T>::I;

  template<typename T>
  using G = class C<T>::I;

  struct D
  {
    template<typename T>
    F<T> f();

    template<typename T>
    G<T> g();
  };
}

namespace tmpl_param_mbr
{
  template<typename T>
  using TI = typename T::I;

  template<typename T>
  using CI = class T::I;

  template<typename T> TI<T> f();

  template<typename T> CI<T> f();
}

namespace nested_classes
{
  // for EDGcpfe/23040
  template<typename T> using I = T;

  template<typename T>
  struct A
  {
    struct B
    {
      template<typename U>
      static B *p1;

      template<typename U>
      static I<B> *p2;

      template<typename U>
      B m1();

      template<typename U>
      I<B> m2();

      template<typename U>
      static B s1();

      template<typename U>
      static I<B> s2();
    };
  };

  struct C
  {
    template<typename T>
    struct B
    {
      template<typename U>
      static B *p1;

      template<typename U>
      static I<B> *p2;

      template<typename U>
      B m1();

      template<typename U>
      I<B> m2();

      template<typename U>
      static B s1();

      template<typename U>
      static I<B> s2();
    };
  };
}
