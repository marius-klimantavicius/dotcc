/*
Test-only observation for the faithful port. Original EDG code is linked from
const_ints.c (Apache-2.0 WITH LLVM-exception); no routine is copied here.
Supervisor only: compile/run, retain commands and sanitizer failures.
This is NOT full frontend initialization and defines no EDG globals or stubs.
*/
#include <cstdio>
#include <cstring>
#include "fe_common.h"

using namespace edg;

static_assert(INTEGER_VALUE_REPR_IS_A_HOST_INTEGER == 0, "multipart profile");
static_assert(BITS_IN_AN_INTEGER_VALUE == 128, "selected capacity");
static_assert(BITS_IN_INT_VALUE_PART == 16, "selected limb width");

static void describe(const char *label, size_t bits)
{
  size_t difference = BITS_IN_AN_INTEGER_VALUE - bits;
  int count = (int)difference;
  int part_offset = count / (int)BITS_IN_INT_VALUE_PART;
  int first_shift = count % (int)BITS_IN_INT_VALUE_PART;
  int second_shift = (int)BITS_IN_INT_VALUE_PART - first_shift;
  std::fprintf(stderr,
    "%s bits=%llu unsigned_difference=%llu int_count=%d part_offset=%d first=%d second=%d\n",
    label, (unsigned long long)bits, (unsigned long long)difference,
    count, part_offset, first_shift, second_shift);
  std::fflush(stderr);
}

static void print_parts(const an_integer_value *value)
{
  for (unsigned i = 0; i < INT_VALUE_PARTS_PER_INTEGER_VALUE; ++i)
    std::fprintf(stderr, "%04x", (unsigned)value->part[i]);
  std::fprintf(stderr, "\n");
  std::fflush(stderr);
}

int main(int argc, char **argv)
{
  const a_targ_size_t configured_width = DEFAULT_BITINT_MAXWIDTH_VALUE;
  const a_targ_size_t char_bits = INTERNAL_TARG_CHAR_BIT;
  const a_targ_size_t storage_size =
                         (configured_width + char_bits - 1) / char_bits;
  const a_targ_size_t storage_bits = storage_size * char_bits;
  std::fprintf(stderr,
    "header-only arithmetic: maxwidth=%llu char_bits=%llu bytes=%llu storage_bits=%llu CHECKING=%d\n",
    (unsigned long long)configured_width, (unsigned long long)char_bits,
    (unsigned long long)storage_size, (unsigned long long)storage_bits, CHECKING);
  describe("signed initializer maximum", (size_t)(storage_bits - 1));
  describe("unsigned initializer maximum", (size_t)storage_bits);
  describe("signed initializer extension", (size_t)storage_bits);
  if (argc == 1) return 0;
  if (argc != 2) return 2;

  an_integer_value value;
  if (std::strcmp(argv[1], "signed-max") == 0) {
    make_integer_value_mask(&value, (size_t)(storage_bits - 1));
  } else if (std::strcmp(argv[1], "unsigned-max") == 0) {
    make_integer_value_mask(&value, (size_t)storage_bits);
  } else if (std::strcmp(argv[1], "signed-extend") == 0) {
    set_integer_value(&value, (a_host_large_integer)1);
    sign_extend_integer_value(&value, (size_t)storage_bits);
  } else if (std::strcmp(argv[1], "valid-127") == 0) {
    make_integer_value_mask(&value, (size_t)127);
  } else if (std::strcmp(argv[1], "valid-128") == 0) {
    make_integer_value_mask(&value, (size_t)128);
  } else {
    return 2;
  }
  print_parts(&value);
  return 0;
}
