//type:fp
//options:--c++11:--c++20:--ms_c++20
//options_all:-w

namespace minimal
{
  template<bool B>
  struct C { };
  template<typename T, typename U>
  int f(U, C<noexcept(U())>);
  int i = f<int>(1, C<true>());
}

namespace noexcept_operator
{
  template<bool B>
  struct C
  { };

  template<typename T, typename U>
  int f(U, C<noexcept(U())>);

  int i = f<int>(1, C<true>());
}

namespace sizeof_expr_operator
{
  template<unsigned B>
  struct C
  { };

  template<typename T, typename U>
  int f(U, C<sizeof(U{})>);

  int i = f<int>(1, C<sizeof(int)>());
}

namespace sizeof_type_operator
{
  template<unsigned B>
  struct C
  { };

  template<typename T, typename U>
  int f(U, C<sizeof(U)>);

  int i = f<int>(1, C<sizeof(int)>());
}

#ifndef _MSC_VER
namespace alignof_expr_operator
{
  template<unsigned B>
  struct C
  { };

  template<typename T, typename U>
  int f(U, C<alignof(U{})>);

  int i = f<int>(1, C<alignof(int)>());
}
#endif

namespace alignof_type_operator
{
  template<unsigned B>
  struct C
  { };

  template<typename T, typename U>
  int f(U, C<alignof(U)>);

  int i = f<int>(1, C<alignof(int)>());
}

#ifdef _MSC_VER
namespace uuidof_expr
{
  template<const _GUID &>
  struct C
  { };

  template<typename T, typename U>
  int f(U, C<__uuidof(U(0))>);

  int i = f<int>(1, C<__uuidof(0)>());
}
#endif
