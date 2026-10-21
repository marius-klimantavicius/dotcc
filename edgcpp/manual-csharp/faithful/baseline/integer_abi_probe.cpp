/*
Test-only ABI observation for the fresh faithful EDG port.
Includes pinned EDG declarations (Apache-2.0 WITH LLVM-exception).
Research child authors this file but MUST NOT compile or execute it.
Supervisor: reuse the selected const_ints.c command's -D/-I/ABI flags,
replace its source/output arguments, and separately retain the command receipt.
This does not link EDG or call its frontend; it observes header-defined types.
*/
#include "basic_hdrs.h"
#include <cstdio>
#include <cstddef>
#include <type_traits>

USING_NAMESPACE_EDG

static_assert(INTEGER_VALUE_REPR_IS_A_HOST_INTEGER == 0,
              "This probe qualifies the pinned multipart configuration only");
static_assert(USE_BOOL_FOR_BOOLEAN_IN_CPLUSPLUS == 1, "boolean configuration");
static_assert(FIXED_POINT_ALLOWED == 1, "fixed-point compile-time feature");
static_assert(INT128_EXTENSIONS_ALLOWED == 1, "int128 compile-time feature");
static_assert(std::is_same<EDG_QUAL an_int_value_part, unsigned short>::value,
              "part typedef");
static_assert(std::is_same<EDG_QUAL a_host_large_integer, long>::value,
              "LP64 host signed typedef");
static_assert(std::is_same<EDG_QUAL a_host_large_unsigned, unsigned long>::value,
              "LP64 host unsigned typedef");
static_assert(std::is_same<EDG_QUAL a_boolean, bool>::value, "boolean typedef");
static_assert(std::is_same<EDG_QUAL a_byte_boolean, bool>::value,
              "byte boolean typedef");
static_assert(INT_VALUE_PARTS_PER_INTEGER_VALUE == 8, "part count");
static_assert(BITS_IN_AN_INTEGER_VALUE == 128, "integer capacity");
static_assert(sizeof(EDG_QUAL an_integer_value) == 16, "integer object size");
static_assert(alignof(EDG_QUAL an_integer_value) == 2, "integer alignment");

int main()
{
  using EDG_QUAL an_integer_value;
  using EDG_QUAL an_int_value_part;
  using EDG_QUAL a_host_large_integer;
  using EDG_QUAL a_host_large_unsigned;
  using EDG_QUAL a_boolean;
  using EDG_QUAL a_byte_boolean;
  unsigned short endian_word = 0x0102;
  unsigned char *endian_bytes = reinterpret_cast<unsigned char *>(&endian_word);
  an_integer_value iv = {};
  std::printf("INTEGER_VALUE_REPR_IS_A_HOST_INTEGER=%d\n", INTEGER_VALUE_REPR_IS_A_HOST_INTEGER);
  std::printf("USE_BOOL_FOR_BOOLEAN_IN_CPLUSPLUS=%d\n", USE_BOOL_FOR_BOOLEAN_IN_CPLUSPLUS);
  std::printf("FIXED_POINT_ALLOWED=%d\n", FIXED_POINT_ALLOWED);
  std::printf("INT128_EXTENSIONS_ALLOWED=%d\n", INT128_EXTENSIONS_ALLOWED);
  std::printf("CHECKING=%d\nDEBUG=%d\n", CHECKING, DEBUG);
  std::printf("CHAR_BIT=%d\nINTERNAL_TARG_CHAR_BIT=%d\n", CHAR_BIT, INTERNAL_TARG_CHAR_BIT);
  std::printf("sizeof_short=%zu\nsizeof_int=%zu\nsizeof_long=%zu\nsizeof_long_long=%zu\n",
              sizeof(short), sizeof(int), sizeof(long), sizeof(long long));
  std::printf("sizeof_pointer=%zu\nsizeof_size_t=%zu\nsizeof_ptrdiff_t=%zu\n",
              sizeof(void *), sizeof(std::size_t), sizeof(std::ptrdiff_t));
  std::printf("sizeof_host_signed=%zu\nalignof_host_signed=%zu\n",
              sizeof(a_host_large_integer), alignof(a_host_large_integer));
  std::printf("sizeof_host_unsigned=%zu\nalignof_host_unsigned=%zu\n",
              sizeof(a_host_large_unsigned), alignof(a_host_large_unsigned));
  std::printf("sizeof_boolean=%zu\nalignof_boolean=%zu\nsizeof_byte_boolean=%zu\n",
              sizeof(a_boolean), alignof(a_boolean), sizeof(a_byte_boolean));
  std::printf("sizeof_part=%zu\nalignof_part=%zu\n", sizeof(an_int_value_part), alignof(an_int_value_part));
  std::printf("part_count=%zu\ninteger_bits=%zu\ninteger_size=%zu\ninteger_alignment=%zu\n",
              static_cast<std::size_t>(INT_VALUE_PARTS_PER_INTEGER_VALUE),
              static_cast<std::size_t>(BITS_IN_AN_INTEGER_VALUE),
              sizeof(an_integer_value), alignof(an_integer_value));
  std::printf("part_offset=%zu\npart_stride=%zu\n",
              offsetof(an_integer_value, part),
              static_cast<std::size_t>(reinterpret_cast<unsigned char *>(&iv.part[1]) -
                                       reinterpret_cast<unsigned char *>(&iv.part[0])));
  std::printf("host_little_endian=%d\npart_promotes_to_int=%d\n",
              endian_bytes[0] == 2,
              std::is_same<decltype(+iv.part[0]), int>::value);
  std::printf("max_part=%u\nsign_bit=%u\n", static_cast<unsigned>(MAX_UINT_VALUE_PART),
              static_cast<unsigned>(SIGN_BIT_INT_VALUE_PART));
  return 0;
}
