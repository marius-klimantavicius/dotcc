//type:fp
//options_all:--c++11
//remark:[4.14] Invalid IL for some array aggregates initialized in conditional operations
// 6/9/17   [EDGcpfe/17901,EDGcpfe/18362]
//
// Invalid IL for some array aggregates initialized in conditional operations
//
// In cases where aggregates are initialized inside a conditional operation and
// the conditional operation is contained within a block scope that is different
// than the temporary it initializes, the local static variable initializer had
// been created in the wrong scope.  In C-generating back end configurations
// this had resulted in an assertion failure (find_local_static_variable_init:
// none found for specified variable and scope).  This regression was introduced
// in 4.12 (by the changes for EDGcpfe/17317).
struct A {
  int x, y;
};
void f(int p) {
  {
    0 ? A{0, 0} : A{p, 0};
  }
}
