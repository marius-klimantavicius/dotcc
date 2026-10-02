//type:fp
//options_all:--c++11
//remark:[4.9] Assertion failure in copy_type_full
// 1/27/14  [EDGcpfe/14812]
//
// Assertion failure in copy_type_full
//
// When using a function with default arguments as the argument to decltype or
// __typeof, an assertion failure in copy_type_full had resulted in some cases
// and is now fixed.
int f(int i = 1);
void g() {
  decltype(f) x;
}
