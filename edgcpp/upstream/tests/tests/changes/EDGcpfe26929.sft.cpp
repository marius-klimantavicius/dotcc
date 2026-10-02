//type:fp
//options_all:--g++
//remark:[6.7] Abort on optimization of virtual call involving default arguments
// 1/11/24  [EDGcpfe/26929]
//
// Abort on optimization of virtual call involving default arguments
//
// The front end previously aborted in lowering because of invalid IL being
// generated.  This invalid IL was a consequence of attempting to optimize the
// virtual call di.f() when the default argument of the statically-determined f
// (B<int>::f in this case) has not been instantiated yet.  This is now fixed.
template<typename> struct B {
  virtual void f(int = 0) const;
};
struct C: B<int> {
  void f(int) const;
};
template<typename T> struct D: C {
  using B<T>::f;  // Nonstandard, but accepted by other compilers.
};
int main() {
   D<int> di;
   di.f();  // Previously elicited an abort during lowering.  Now okay.
}
