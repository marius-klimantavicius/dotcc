//type:fp
//remark:[4.1] Infinite loop with delayed nested class in a local class member function
// 3/18/09  [EDGcpfe/9626]
//
// Infinite loop with delayed nested class in a local class member function
//
// Lowering of a delayed nested class (i.e., the definition appears after
// a declaration in a nested class) in a local class member function could
// result in an infinite loop in promote_type_list.  Now fixed.
struct A {
  void g() {
    struct B {
      struct C;
    };
    struct B::C {
      void f() {}
    } x;
    x.f();
  }
} a;
int main() {
  a.g();
}
