//type:fp
//options_all:--clang
//remark:[5.1] Clang compatibility: Spurious error with designated initializers
// 6/24/19  [EDGcpfe/21440]
//
// Clang compatibility: Spurious error with designated initializers
//
// Clang accepts designated initializers for members of anonymous structs/unions
// without an extra layer of braces, however the front end was spuriously
// rejecting these cases.
//
// This is now fixed.
struct A {
  struct {
    char c;
  };
};
void f() {
  A a = { .c = 1 }; // Spurious error requiring an extra layer of braces
}
