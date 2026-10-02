//type:fp
//options_all:--c++11 --g++ -tused
//remark:[6.2] Abort with default arguments to inheriting constructors
// 10/5/20  [EDGcpfe/23298]
//
// Abort with default arguments to inheriting constructors
//
// In configurations where IL lowering is performed, the front end could abort in
// lower_dynamic_init when lowering code where a default argument is provided to
// a constructor that has been inherited, and that inheriting constructor is being
// used.
//
// This is now fixed.
struct A {
  ~A() { }
};
template <class T> struct B {
  B(int, A = A()) { }
};
struct C : B<int> {
  using B<int>::B;
};
template <class T> void f() {
  C c(1); // Abort triggered here
}
void g() {
  f<int>();
}
