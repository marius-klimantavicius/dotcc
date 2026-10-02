//type:fp
//options_all:--gn 80300
//remark:[6.4] Access checking with using-declarations
// 5/17/22  [EDGcpfe/24818]
//
// Access checking with using-declarations
//
// In some cases, the front end incorrectly handled access checking for class
// members in inheritance hierarchies containing member using-declarations,
// either reporting spurious access errors or failing to report legitimate
// access violations.  This is now fixed.
struct B1 { int i; };
struct B2 : B1 {
  friend void f();
private:
  using B1::i;
};
struct D : B2 { };
struct E : D { };
struct Ptr {
  E* operator->();
};
Ptr p;
void f() {
  p->i;   // Previously incorrectly reported access error for "i"
}
