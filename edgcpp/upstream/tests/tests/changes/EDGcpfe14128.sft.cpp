//type:fp
//options_all:--microsoft_version=1700
//remark:[4.8] Microsoft compatibility: accept "final" function modifier
// 7/3/13   [EDGcpfe/14128]
//
// Microsoft compatibility: accept "final" function modifier
//
// The "final" context sensitive keyword is now accepted as a member
// function modifier when microsoft_version >= 1700.
// --microsoft_version=1700):
struct B {
  virtual void f() {}
};
struct D : B {
  virtual void f() final {}
};
