#include "project_cpp_comdat_storage.h"
int main() {
  if (provider_inline_address() != &inline_value) return 1;
  if (provider_template_address() != &SharedStorage<int>::value) return 2;
  if (provider_member_address() != &StaticStorage::value) return 3;
  if (mutate_shared_storage() != 34) return 4;
  if (inline_value != 5 || SharedStorage<int>::value != 20 || StaticStorage::value != 9) return 5;
  *template_address() = 31;
  if (*provider_template_address() != 31) return 6;
  return 0;
}
