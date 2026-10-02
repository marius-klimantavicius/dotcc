//type:fn
//options_all:--c --gcc
//remark:[4.2] GNU C compatibility: Flexible array member initializer constraints
// 6/4/10   [EDGcpfe/10570]
//
// GNU C compatibility: Flexible array member initializer constraints
//
// In GNU C mode, the front end accepts aggregate initializers for flexible array
// members (see Changes entry of 9/1/04).  Now, an additional constraint is
// enforced (to match the behavior of GCC): The variable being initialized must
// have static storage duration.
typedef struct S { int n; int a[]; } S;
void g() {
  S s1 = { 1, { 2 } };  // Previously accepted in GNU C mode; now an error.
  static S s2 = { 3, { 4 } };  // Still accepted in GNU C mode.
}
