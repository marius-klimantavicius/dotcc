//type:fp
//options_all:--c++17 --gnu=100000
//remark:[6.7] Assertion failure when lowering some aggregate constants
// 4/16/24  [EDGcpfe/27115]
//
// Assertion failure when lowering some aggregate constants
//
// In certain modes, the lowering of an aggregate constant with a designated
// initializer that initializes a type with a field with a [[no_unique_address]]
// attribute, had caused an assertion failure (in
// advance_aggregate_position_to_next_member or in some cases
// fill_out_aggregate_ptr_to_data_member_initialization) and is now fixed.
struct A {};
struct B {
  [[no_unique_address]] A a;
  int i;
};
void f() {
  auto b = B { .i = 512 };
}
auto b = B { .i = 512 };
