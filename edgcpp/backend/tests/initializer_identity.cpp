#include "initializer_identity.h"
#include <string.h>
#ifdef IDENTITY_NATIVE_MAIN
#include <stdio.h>
#endif

static unsigned long representation(double value) {
  unsigned long bits;
  static_assert(sizeof(bits) == sizeof(value), "fixture expects LP64");
  memcpy(&bits, &value, sizeof(bits));
  return bits;
}

extern "C" int initializer_identity_verify() {
  const void* expected[] = {&identity_first, &identity_second, &identity_second,
      &identity_second, identity_array, identity_array + 1, identity_array + 1,
      &identity_pair.first, &identity_pair.second, nullptr};
  for (unsigned i = 0; i < 10; ++i) {
    if (identity_rows[i].address != expected[i]) return 10 + static_cast<int>(i);
    if (identity_rows[i].size != (i == 9 ? 0UL : 8UL)) return 30 + static_cast<int>(i);
  }
  if (identity_rows[0].address == identity_rows[1].address ||
      identity_rows[4].address == identity_rows[5].address ||
      identity_rows[7].address == identity_rows[8].address) return 50;
  for (const auto& row : identity_repeated)
    if (row.address != &identity_first || row.size != 8 || row.flags != 19) return 51;
  if (identity_functions[0].function != identity_function_first ||
      identity_functions[1].function != identity_function_first ||
      identity_functions[2].function != identity_function_second) return 52;
  if (identity_functions[0].function() != 13 || identity_functions[2].function() != 17) return 53;
  const unsigned long expected_bits[] = {0UL, 0x8000000000000000UL,
      0x8000000000000000UL, 0x8000000000000000UL};
  for (unsigned i = 0; i < 4; ++i) {
    if (representation(identity_floats[i].value) != expected_bits[i]) return 60 + static_cast<int>(i);
    if (identity_floats[i].address != (i == 3 ? static_cast<void*>(&identity_second) : static_cast<void*>(&identity_first))) return 70 + static_cast<int>(i);
  }
  if (identity_envelopes[0].payload.address != &identity_first ||
      identity_envelopes[1].payload.address != &identity_second ||
      identity_envelopes[2].payload.address != &identity_second) return 80;
  if (representation(identity_envelopes[3].payload.value) != 0UL ||
      representation(identity_envelopes[4].payload.value) != 0x8000000000000000UL ||
      representation(identity_envelopes[5].payload.value) != 0x8000000000000000UL) return 81;
  return 0;
}

#ifdef IDENTITY_NATIVE_MAIN
int main() {
  int result = initializer_identity_verify();
  printf("initializer identity %d\n", result);
  return result;
}
#endif
