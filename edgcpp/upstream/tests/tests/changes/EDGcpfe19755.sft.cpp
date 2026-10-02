//type:fp
//options_all:--c++14 -A
//remark:[5.0] Referring to local variables from nested local types
// 6/20/18  [EDGcpfe/19755]
//
// Referring to local variables from nested local types
//
// The front end now more broadly permits referring to local variables from
// nested local types if that reference is from an unevaluated expression
// context.
//
// As part of this change, member functions of local class types are now stored
// in the memory region of the enclosing function (as was already the case for
// the member functions of local closure classes).
void g() {
  int x = 1;
  struct S {
    void f() { decltype(x) y; }  // Previously an error.  Now okay.
  };
}
