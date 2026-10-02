//type:fp
//remark:[4.4] By-reference capture across a non-mutable lambda
// 10/18/11 [EDGcpfe/12317]
//
// By-reference capture across a non-mutable lambda
//
// When an inner lambda captures a local variable by reference across an outer
// non-mutable lambda that captures that variable by value, the field modeling
// the capture in the inner lambda is now a reference to a const type.
// Previously, this was not the case, which resulted in spurious errors.
//
// (This agrees with a pending clarification for core issue 1249.)
int main() {
  int i = 42;
  auto outer = [i]{
    auto inner = [&i]{};  // Previously triggered an error about binding
  };                      // a const i to a non-reference.
}
