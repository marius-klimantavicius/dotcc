//type:fp
//remark:[4.1] Invalid generated C code for some delayed nested class definitions
// 3/13/09 [EDGcpfe/9612]
//
// Invalid generated C code for some delayed nested class definitions
//
// In some cases, delayed nested class definitions (accepted in GNU and
// Microsoft modes) resulted in generated C code that produced errors when
// compiled.  This is now fixed.
struct A {
  struct B {
    struct C;
  } b;
  struct B::C {     // delayed nested class definition
    void f() {}
  } x;              // had generated "error: field `x' has incomplete type"
} a;                // diagnostic when compiling generated C

int main() {
  a.x.f();
}
