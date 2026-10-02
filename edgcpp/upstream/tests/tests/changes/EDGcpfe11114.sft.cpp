//type:fp
//options_all:--g++ --gnu=30400
//remark:[4.3] Abort on promotion of bit field with template-dependent type
// 11/2/10  [EDGcpfe/11114]
//
// Abort on promotion of bit field with template-dependent type
//
// The front end aborted on attempting to determine the promoted type for
// a bit field with a dependent type, in modes that do prototype instantiations.
// Now fixed.
template <class T> struct A {
  enum E { e };
  E m:8;
  void f() {
    switch (m) {}
  }
};
int main() {
  A<int> a;
  a.f();
}
