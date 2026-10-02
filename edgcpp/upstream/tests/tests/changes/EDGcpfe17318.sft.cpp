//type:fp
//remark:[4.13] Member using-declarations and class/enum names
// 11/3/16  [EDGcpfe/17318]
//
// Member using-declarations and class/enum names
//
// Previously, the front end issued an error when a member using-declaration
// referred to function declaration from a base class in a derived class with a
// class or enum type of the same name.
//
// This is now fixed.
struct B { void X(); };
struct D: B {
  struct X {};
  using B::X;  // Previously an error.  Now accepted.
};
