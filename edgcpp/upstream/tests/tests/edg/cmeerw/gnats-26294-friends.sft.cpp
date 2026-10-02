//type:fn
//options:--c++20

namespace minimal
{
  template<typename T> int f(T) = delete;
  template<typename T> int f(T);
  int i = f(0);                 // error
}

namespace redecl_deleted_template_function
{
  template<typename T>
  int f(T) = delete;

  template<typename T>
  int f(T);

  int i = f(0);                 // error
}

namespace friend_for_deleted_template_function
{
  template<bool>
  struct A
  { };

  template<bool B1>
  struct D;

  template<bool B3>
  int g(D<true> const &, A<B3>) = delete;

  template<bool B1>
  struct D
  {
    template<bool B3>
    friend int g(D const &, A<B3>);
  };

  int i = g(D<true>(), A<true>()); // error
}
