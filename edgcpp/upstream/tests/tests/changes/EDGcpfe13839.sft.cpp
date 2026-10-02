//type:fp
//options_all:--c++11
//remark:Incorrect field used for anonymous unions in lowered subobject classes
// 5/7/26   [EDGcpfe/13839,EDGcpfe/22461,EDGcpfe/28774]
//
// Incorrect field used for anonymous unions in lowered subobject classes
//
// The lowering process adjusts expressions that refer to anonymous union fields
// and in doing so had inadvertently used fields in a complete object class
// rather than the subobject class.  This invalid IL could cause problems for
// back ends and had resulted in an assertion failure
// ("dump_field_from_second_operand: wrong field class") in C-generating back end
// configurations configured for the IA-64 ABI.
struct A {
  virtual void f();
  union {int x = 37;};
};
struct B : A {
  constexpr B() {}
};
auto x = new B[0]{ {} };
