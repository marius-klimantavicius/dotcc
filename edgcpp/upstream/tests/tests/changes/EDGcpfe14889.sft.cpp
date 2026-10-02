//type:fp
//remark:[4.10] Using constant-valued variables in lambda expressions
// 4/23/14  [EDGcpfe/14889]
//
// Using constant-valued variables in lambda expressions
//
// Previously, the front end issued an error when a lambda expression used the
// value of a constant-valued variable declared in a function outside that
// lambda expression if the variable is not captured.
//
// Such cases are now accepted (in modes that accept lambda expressions).
int g() {
  int const N = 3;
  return []{ return N; }();  // Previously an error; now okay.
}
