//type:fp
//options:--c++11 -A:--c++11 --microsoft_version 1944

namespace minimal
{
  template<typename T, typename ...>
  struct B {
    template<typename U = T>
    void foo();
  };
  template<typename ... Ts>
  struct D : B<Ts ...>
  { };
}

#if _MSC_VER
namespace instantiate_dpdt_base
{
  template<typename ...>
  struct C
  { };

  int N;

  template<typename T>
  struct B
  {
    struct N
    { };
  };

  template<typename ... Ts>
  struct D : B<C<Ts ...>>
  {
    N n;
  };
}
#endif
