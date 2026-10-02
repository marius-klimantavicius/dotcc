//type:fp
//options:-DNO_ERRORS:-DERRORS;fn
//options_all:--c++23 -A

namespace A {
  extern "C" void f(int, int = 5);
  extern "C" void f(int = 6, int);
}
namespace B {
  extern "C" void f(int, int = 7);
}

void use() {
  using A::f;
  using B::f;

  f(3, 4);           // OK, default argument was not used for viability
#ifdef ERRORS
  f(3);              // error: default argument provided by declaration from two scopes
#endif
  f();               // OK, default arguments provided by declarations in the scope of A

  void g(int = 8);
  g();               // OK
}

void h(int = 7);
void poison() {
  void h(int = 8);
  h();       // ok, calls h(8)
}

template <typename... Ts>
int k(int = 3, Ts...);
#ifdef ERRORS
int i = k<int>();  // error: no default argument for the second parameter
#endif
int j = k<>();     // OK

namespace A { extern int z[3]; }
int A::z[] = {};   // OK, defines an array of 3 elements

//cwg: 2701
//title: Default arguments in multiple scopes / inheritance of array bounds in the same scope
//meeting: Sofia 6/25
//edg_status: EDGcpfe/28251
