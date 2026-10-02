//type:fp
//options_all:--c++14
//remark:[4.10.1] Assertion failure during lowering of multidimensional arrays
// 2/3/15   [EDGcpfe/15963]
//
// Assertion failure during lowering of multidimensional arrays
//
// An assertion failure ("lower_aggregate_designated_initializers: type mismatch")
// had occurred in certain cases when lowering multidimensional arrays that
// have at least three dimensions and the last two dimensions both have a
// single element.
struct A {
  struct B {
    int y = 37;
  } a[2][1][1] {};
};
struct C : A {
  union {
    char a;
    int x = 37;
  };
};
int main() {
  return C().x != 37;
}
