//type: fp
//options: 
# 0 "./debug/dwarf2/template-params-12w.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./debug/dwarf2/template-params-12w.C"



# 1 "./debug/dwarf2/template-params-12.H" 1

struct B {
  void g();
  virtual void v() = 0;
  virtual void w();
};
void B::g() {}
void B::w() {}
struct S : B {
  void f();
  void v();
  void u();
};
void S::f() {}
void S::v() {}
template <void (B::*MF)()> void t() {}
template <void (S::*MF)()> void t() {}
# 5 "./debug/dwarf2/template-params-12w.C" 2

template void t<&S::v>();
