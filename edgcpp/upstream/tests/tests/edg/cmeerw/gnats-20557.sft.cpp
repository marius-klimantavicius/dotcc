//type:fp
//options:--c++11

template<typename T>
struct C
{ };

namespace builtin_trait_as_template_arg
{
  template<typename T> void f(T, C<__remove_const(T)>) { }
  template<typename T> void f(T, C<__remove_cv(T)>) { }

  template<typename T> void f(T, C<const __remove_volatile(T)>) { }
  template<typename T> void f(T, C<const __remove_cv(T)>) { }

  template<typename T> void f(T, C<const __remove_const(T)>) { }
  template<typename T> void f(T, C<volatile __remove_const(T)>) { }

  template<typename T> void f(T, C<__remove_const(decltype(T()))>) { }
  template<typename T> void f(T, C<__remove_cv(decltype(T()))>) { }

  template<typename T> void f(T, C<__remove_const(decltype(T(1)))>) { }
  template<typename T> void f(T, C<__remove_const(decltype(T(2)))>) { }
}

namespace builtin_trait_with_decltype_operand
{
  template<typename T> void f(T, __remove_const(decltype(T()))) { }
  template<typename T> void f(T, __remove_cv(decltype(T()))) { }

  template<typename T> void f(T, __remove_const(decltype(T(1)))) { }
  template<typename T> void f(T, __remove_const(decltype(T(2)))) { }

  struct D
  {
    template<typename T> void f(T, __remove_const(decltype(T())));
  };

  template<typename T> void D::f(T, __remove_const(decltype(T())))
  { }
}

namespace const_qualified_after_dependent_type_operator
{
  template<typename T> void f(T, __remove_const(const T)) { }
  template<typename T> void f(T, __remove_cv(const T)) { }
}

namespace const_qualified_dependent_type_operator_reference
{
  template<typename T> void f(T, const __remove_const(T) &) { }
  template<typename T> void f(T, const __remove_cv(T) &) { }
}

namespace remove_const_and_plain_type
{
  template<typename T> void f(T, __remove_const(T)) { }
  template<typename T> void f(T, T) { }
}

namespace underlying_type_and_plain_type
{
  template<typename T> void f(T, __underlying_type(T)) { }
  template<typename T> void f(T, T) { }
}

namespace non_dpdt_type_operator_equivalent
{
  struct D
  {
    void f(int, __remove_const(int));
  };

  void D::f(int, __remove_volatile(int))
  { }
}
