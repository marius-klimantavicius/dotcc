//type:fp
//options_all:--g++ --c++23
//remark:Internal error in discard_constant_expr_object_lifetime
// 4/6/26   [EDGcpfe/28775]
//
// Internal error in discard_constant_expr_object_lifetime
//
// Previously, this aborted in discard_constant_expr_object_lifetime (exprutil.c,
// with an internal error) because it was assumed that object lifetime entries in
// a constant context could only appear during error recovery.  That erroneous
// assumption has now been dropped.
struct S {
  int i;
  constexpr ~S() {}
};
constexpr S g(bool b) {
  if consteval {
    return b ? S() : S();
  }
  return S();
}
int main() {
  int arr[g(true).i];
}
