//type:fp
//remark:[4.1] C++-generating back end: abort with "auto" type specifier
// 1/30/09  [EDGcpfe/9494]
//
// C++-generating back end: abort with "auto" type specifier
//
// In configurations in which IL_SHOULD_BE_WRITTEN_TO_FILE is TRUE and
// PROTOTYPE_INSTANTIATIONS_IN_IL is FALSE, a local variable defined with the
// "auto" type specifier could cause an abort in the C++-generating back end.
// Now fixed.
void f() {
  auto x = 5;    // Previously aborted generating this definition
}
