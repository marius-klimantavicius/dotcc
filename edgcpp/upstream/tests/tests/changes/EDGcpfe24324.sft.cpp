//type:fp
//options_all:--c++20
//remark:[6.3] Constexpr comparison of pointers into dynamically-allocated storage
// 6/16/21  [EDGcpfe/24324]
//
// Constexpr comparison of pointers into dynamically-allocated storage
//
// In C++20, constexpr evaluation can involve dynamic allocation.  Previously, a
// bug in the constexpr address comparison code could result in an internal error
// (in find_subobject_for_interpreter_address) when comparing pointers to a class
// type subobject into such dynamically-allocated storage.
//
// That problem is now fixed.
struct S { int value; };
constexpr bool g() {
  S *p = new S[10];
  bool r = p <= p + 10;  // Previously triggered an internal error in
  delete[] p;            // C++20 mode.  Now okay.
  return r;
}
static_assert(g());
