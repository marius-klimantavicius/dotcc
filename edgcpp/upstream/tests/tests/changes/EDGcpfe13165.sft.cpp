//type:fp
//options_all:--g++
//remark:[4.5] Incorrect lowering of compound literals or initializer-list constants
// 9/6/12   [EDGcpfe/13165]
//
// Incorrect lowering of compound literals or initializer-list constants
//
// During the lowering of compound literals or initializer-list constants,
// the lowered code to perform the requisite initialization may have been
// emitted out of order, resulting in undefined behavior at runtime (or in older
// versions, an assertion failure: "lower_dynamic_init: keep_dynamic_init
// param NULL and want to return TRUE").
//
// A change was also made to the handling of stmk_init statements created
// during the lowering of NULL pointer-to-data-member constants in IA-64
// ABI configurations.
struct B {
  B(int i) : i(i) {}
  ~B() {};
  int i;
};
struct A {
  int x;
  B y;
};
int f(A p) {
  return p.x + p.y.i;
}
int y = f((A){37, B(47)});
int main() {
  return y != 84;   // Now returns zero.
}
