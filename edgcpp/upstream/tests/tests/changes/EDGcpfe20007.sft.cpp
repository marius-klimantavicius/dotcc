//type:fp
//options_all:--c++20
//remark:[6.0] C++20: Range-based-for statements with initializer
// 10/30/19 [EDGcpfe/20007,EDGcpfe/21948]
//
// C++20: Range-based-for statements with initializer
//
// An optional initializer statement is now allowed in a range-based-for
// statement in C++20 mode.  See P0614R1 for details.
void f() {
  int sum;
  int a[] = {1,2};
  for (sum = 0; auto y: a)
    sum += y;
}
