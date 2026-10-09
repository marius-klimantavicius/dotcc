#include "memory_asm.h"

// Keep caller and assembly provider as distinct native translation units.
// An '=m' constraint permits the compiler to discard the operand's old value;
// the caller must initialize its bytes outside that compilation boundary.
int main() {
  unsigned short storage[32];
  unsigned char *bytes = (unsigned char *)storage;
  for (unsigned i = 0; i != sizeof(storage); ++i) bytes[i] = (unsigned char)(i * 3 + 5);
  reset_observations();
  empty_barrier();
  if (address_calls || extent_calls) return 1;
  input_scalar(bytes + 1);
  output_scalar(bytes + 3);
  inout_scalar(bytes + 5);
  input_fixed_matrix(bytes + 8);
  multiple_operands(bytes + 23);
  if (address_calls != 7 || extent_calls) return 2;

  for (unsigned count = 1; count <= 17; ++count) {
    reset_observations();
    input_variable_extent(bytes + 1, count);
    input_variable_wide_extent(bytes + 2, count);
    inout_variable_extent(bytes + 3, count);
    if (address_calls != 3 || extent_calls != 3 || extent_total != count * 3) return 3;
  }
  unsigned char *cursor = bytes + 7;
  increment_address(&cursor);
  if (cursor != bytes + 8) return 4;
  for (unsigned i = 0; i != sizeof(storage); ++i)
    if (bytes[i] != (unsigned char)(i * 3 + 5)) return 5;
  BarrierPayload payload = { 0x12345678U, 1.25 };
  record_operand(&payload);
  if (payload.value != 0x12345678U || payload.real != 1.25) return 6;
  return 0;
}
