//options_all:--c++23 -A
enum E { x };
void f() {
  int E;
  using enum E;   // OK
}
using F = E;
using enum F;     // OK
template<class T> using EE = T;
void g() {
  using enum EE<E>;  // OK
}

//cwg: 2877
//title: Type-only lookup for using-enum-declarator
//meeting: St Louis 6/24
//edg_status: Passes
