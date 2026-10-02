//type:fp
//options:--c++14:--c++20:--ms_c++20

namespace minimal
{
  template<typename T> auto f() -> decltype(T::v);
  template<typename T> int f();
  struct A {
    template<typename> static int v;
  };
  int i = f<A>();
}

namespace sfinae
{
  template<typename T>
  auto f() -> decltype(T::v);

  template<typename T>
  auto f() -> decltype(T::template v<int>);

  struct A
  {
    template<typename> static int v;
  };

  int i = f<A>();
}

#if __cpp_concepts
namespace constraint
{
  template<int>
  struct X
  { };

  template<typename T> requires T::template v<int>
  X<1> f(T);

  template<typename T> requires T::v
  X<2> f(T);

  struct C1
  {
    template<typename T>
    static constexpr bool v = true;
  };

  struct C2
  {
    static constexpr bool v = true;
  };

  X<1> x1 = f(C1());
  X<2> x2 = f(C2());
}

namespace constraint_base_class
{
  template<int>
  struct X
  { };

  template<typename T> requires T::template v<int>
  X<1> f(T);

  template<typename T> requires T::v
  X<2> f(T);

  struct C1
  {
  protected:
    template<typename T>
    static constexpr bool v = true;
  };

  struct D1 : C1
  {
    using C1::v;
  };

  struct C2
  {
  protected:
    static constexpr bool v = true;
  };

  struct D2 : C2
  {
    using C2::v;
  };

  X<1> x1 = f(D1());
  X<2> x2 = f(D2());
}
#endif

namespace sfinae_base_class
{
  template<int>
  struct X
  { };

  template<typename T>
  auto f(T) -> decltype(T::template v<int>);

  template<typename T>
  auto f(T) -> decltype(T::v);

  struct C1
  {
  protected:
    template<typename T>
    static X<1> v;
  };

  struct D1 : C1
  {
    using C1::v;
  };

  struct C2
  {
  protected:
    static X<2> v;
  };

  struct D2 : C2
  {
    using C2::v;
  };

  X<1> x1 = f(D1());
  X<2> x2 = f(D2());
}

namespace fn_addr
{
  template<int>
  struct X
  { };

  template<typename T>
  auto f(T) -> decltype(&T::template fn<int>);

  template<typename T>
  auto f(T) -> decltype(&T::fn);

  struct C1
  {
  protected:
    template<typename T>
    static X<1> fn();
  };

  struct D1 : C1
  {
    using C1::fn;
  };

  struct C2
  {
  protected:
    static X<2> fn();
  };

  struct D2 : C2
  {
    using C2::fn;
  };

  X<1> (*x1)() = f(D1());
  X<2> (*x2)() = f(D2());
}
