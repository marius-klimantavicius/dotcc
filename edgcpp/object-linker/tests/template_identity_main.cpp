#define TEMPLATE_IDENTITY_WIDE_LOCAL
#include "template_identity_shared.h"
static Named::Box<LocalSame, 5> local_same = { { 17 } };
static Named::Box<LocalDifferent, 5> local_different = { { 19 } };
static Named::Box<decltype(private_local_value()), 9> local_private_function = { { 73 } };
int main() {
  Named::Box<int, 3> value_int = { 23 };
  Named::Box<long, 3> value_long = { 29 };
  Named::Box<int, 4> value_four = { 31 };
  Other::Box<int, 3> value_other = { 37 };
  Outer::Named::Box<int, 3> value_nested = { 41 };
  if (shared_address() != &shared_box || shared_box.value != 7) return 1;
  shared_address()->value = 43;
  if (shared_box.value != 43 || read_int(&shared_box) != 43) return 2;
  if (read_int(&value_int) != 23 || read_long(&value_long) != 29 ||
      read_four(&value_four) != 31 || read_other(&value_other) != 37 ||
      read_nested(&value_nested) != 41) return 3;
  if (provider_same_address() == &local_same ||
      provider_different_address() == &local_different) return 4;
  if (change_provider_locals() != 32) return 5;
  if (local_same.value.value != 17 || local_different.value.value != 19) return 6;
  SharedLocalValue direct = provider_shared_local();
  SharedLocalCallback callback = provider_local_callback();
  SharedLocalValue indirect = callback();
  Named::Box<decltype(shared_local_value()), 7> shared_local_copy = { { 59 } };
  if (direct.value != 47 || indirect.value != 47 || shared_local_box.value.value != 53 ||
      read_shared_local_box(&shared_local_copy) != 59) return 7;
  shared_local_box.value.value = 61;
  if (read_shared_local_box(&shared_local_box) != 61 ||
      shared_local_box_address() != &shared_local_box || provider_read_shared_local_box() != 61) return 8;
  auto private_value = private_local_value();
  if (private_value.value != 67 || local_private_function.value.value != 73 ||
      provider_private_function_address() == &local_private_function) return 9;
  return 0;
}
