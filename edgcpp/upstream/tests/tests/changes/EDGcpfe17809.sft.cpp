//type:fp
//remark:C++-generating back end: friend declaration in local class
// 1/22/26  [EDGcpfe/17809]
//
// C++-generating back end: friend declaration in local class
//
// The C++-generating back end previously aborted with a failed assertion in
// gen_declaration_statement when a local class contains a friend declaration.
// This is now fixed.
void f() {
  class C;
  class X1 {
    friend C;   // Previously caused an assertion failure
  };
}
