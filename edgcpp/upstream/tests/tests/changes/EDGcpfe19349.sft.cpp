//type:fp
//options_all:--c++11
//remark:[6.8] Abort on nested lambda in default member initializer
// 10/1/25  [EDGcpfe/19349,EDGcpfe/20562,EDGcpfe/21710,EDGcpfe/22775,
//           EDGcpfe/26431]
//
// Abort on nested lambda in default member initializer
//
// Previously, a lambda expression nested in a default argument of another lambda
// expression, which is itself used in a default member initializer, resulted in
// an incorrect variant access, likely leading to aborts and other incorrect
// behavior.
class A {
  int x = [](int i = [] { return 1; }()) { return 2; }();
};
