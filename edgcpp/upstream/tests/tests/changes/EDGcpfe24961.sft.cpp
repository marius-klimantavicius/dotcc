//type:fp
//options_all:--c++17 --clang_version=110000
//remark:[6.4] Abort in base_class_rvalue_expr
// 1/3/22   [EDGcpfe/24961]
//
// Abort in base_class_rvalue_expr
//
// The changes for EDGcpfe/18083 (in version 6.3) introduced a regression in some
// cases involving a constexpr conversion function, causing the front end to abort
// with an internal error in base_class_rvalue_expr (il.c).
//
// In this example, the problem occurred while overload resolution checked whether
// the template argument d can be converted to the type E of the corresponding
// template parameter: That requires tentatively converting a class lvalue as a
// constant using an inherited conversion function, which the front end did not
// handle correctly.  That problem is now fixed.
struct B {
  int i;
  enum class E : int {};
  constexpr operator E() const {
    return static_cast<E>(i);
  }
  template<E> static constexpr bool f() {
      return true;
  }
};
struct D: public B {};
constexpr D d{42};
static_assert(D::f<d>());  // Previously triggered an abort.  Now okay.
