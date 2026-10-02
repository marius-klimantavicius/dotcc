//type:fp
//options_all:-W --c++23
//remark:Spurious warning on returning a reference from a lambda
// 4/10/26  [EDGcpfe/28684]
//
// Spurious warning on returning a reference from a lambda
//
// The front end previously issued a warning about returning a reference to a
// local variable.  However, in this case the local variable is captured from an
// enclosing function, which makes the construct more likely to be valid.  The
// front end therefore no longer warns about such cases.
int g() {
  int i = 42;
  return [&]()->int& { return i; }();
}
