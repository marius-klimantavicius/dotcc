#include "template_identity_shared.h"
Named::Box<int, 3> shared_box = { 7 };
void* shared_local_box_address() { return &shared_local_box; }
int provider_read_shared_local_box() { return read_shared_local_box(&shared_local_box); }
static Named::Box<LocalSame, 5> local_same = { { 11 } };
static Named::Box<LocalDifferent, 5> local_different = { { 13 } };
static Named::Box<decltype(private_local_value()), 9> local_private_function = { { 71 } };
Named::Box<int, 3>* shared_address() { return &shared_box; }
int read_int(Named::Box<int, 3>* p) { return p->value; }
long read_long(Named::Box<long, 3>* p) { return p->value; }
int read_four(Named::Box<int, 4>* p) { return p->value; }
int read_other(Other::Box<int, 3>* p) { return p->value; }
int read_nested(Outer::Named::Box<int, 3>* p) { return p->value; }
void* provider_same_address() { return &local_same; }
void* provider_different_address() { return &local_different; }
void* provider_private_function_address() { return &local_private_function; }
int change_provider_locals() {
  local_same.value.value += 3;
  local_different.value.value += 5;
  return local_same.value.value + local_different.value.value;
}
