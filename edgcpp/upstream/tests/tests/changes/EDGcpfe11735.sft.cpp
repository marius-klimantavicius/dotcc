//type:fp
//remark:[4.4] Assertion failure in optimize_node_if_possible during lowering
// 5/18/11  [EDGcpfe/11735]
//
// Assertion failure in optimize_node_if_possible during lowering
//
// An assertion failure had occurred (in optimize_node_if_possible) during
// the lowering of certain assignment statements (involving const-qualified
// classes with tail padding in the IA-64 ABI) and is now fixed.
struct A {
  void* p;
  char c;
  A() {}
};
const A &f();
void g(bool b) {
  A a;
  a = b ? f() : a;
}
