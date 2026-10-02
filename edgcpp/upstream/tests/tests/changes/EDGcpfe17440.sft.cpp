//type:fp
//options_all:--c++11
//remark:[5.0] Function modifiers and parenthesized member declarators
// 2/21/18  [EDGcpfe/17440,EDGcpfe/18755]
//
// Function modifiers and parenthesized member declarators
//
// The front end previously did not correctly parse function modifiers like
// "final" when the underlying function declarator is parenthesized.
//
// This is now fixed.
struct S {
  virtual int (f()) final;  // Previously a spurious error.  Now okay.
};
