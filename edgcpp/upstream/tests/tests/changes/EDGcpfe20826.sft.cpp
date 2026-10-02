//type:fp
//options_all:--c++20
//remark:[6.1] Internal error: add_field: two fields have the same offset
// 7/14/20  [EDGcpfe/20826]
//
// Internal error: add_field: two fields have the same offset
//
// In configurations that do lowering and have CHECKING enabled, an internal
// error had occurred when adding the _vptr field into a class that has a member
// with the [[no_unique_address]] attribute.
struct A {
  virtual void f();
  [[no_unique_address]] struct B {} b;
};
