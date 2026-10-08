#include "project_cpp_comdat_storage.h"
int mutate_shared_storage() {
  inline_value += 2;
  SharedStorage<int>::value += 3;
  StaticStorage::value += 4;
  return inline_value + SharedStorage<int>::value + StaticStorage::value;
}
int* provider_inline_address() { return &inline_value; }
int* provider_template_address() { return template_address(); }
int* provider_member_address() { return &StaticStorage::value; }
