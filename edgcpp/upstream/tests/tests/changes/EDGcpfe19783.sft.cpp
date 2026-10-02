//type:fn
//options_all:--c++11
//remark:[6.4] Pointers to subobjects as nontype template arguments for function templates
// 4/26/22  [EDGcpfe/19783]
//
// Pointers to subobjects as nontype template arguments for function templates
//
// The C++ Standard requires that the argument for a nontype template
// parameter of type pointer or reference to object must designate a complete
// object.  The front end previously failed to enforce that rule for pointers
// in explicit template arguments for function templates.  This is now fixed.
struct B { };
struct D : B {} d;
template <const B*> void f();
void g() {
  constexpr B *bp = &d;
  f<bp>();  // Previously incorrectly accepted, now an error
}
