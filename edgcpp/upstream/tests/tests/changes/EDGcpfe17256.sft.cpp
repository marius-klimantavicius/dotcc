//type:fp
//options_all:--c++11
//remark:[4.12] Constant folding of references to functions
// 6/8/16   [EDGcpfe/17256]
//
// Constant folding of references to functions
//
// In some cases the front end incorrectly handled constant expressions
// involving references to functions, leading to spurious errors.  This is
// now fixed.
using fptr = void(*)();
struct A {
  constexpr A(fptr f) : m(f) { }
  fptr m;
};
template<class T> constexpr A make(T&& t) {
  return A(t);
}
constexpr fptr&& get(A&& a) {
  return static_cast<fptr&&>(a.m);
}
void f() { }
void g() {
  get(make(f)) == f;  // Previously an error
}
