// Test-only instrumentation. Calls the original EDG routines; no semantic
// implementation is copied here. Never linked to the production C# library.
#include <cstdio>
#include <cstring>
#include <cstdlib>
#include "fe_common.h"

using namespace edg;

static void read_parts(const char *text, an_integer_value *value)
{
  for (int i = 0; i != 8; ++i) {
    char part[5] = {};
    std::memcpy(part, text + i * 4, 4);
    value->part[i] = (an_int_value_part)std::strtoul(part, nullptr, 16);
  }
}

static void print_parts(const an_integer_value *value)
{
  for (int i = 0; i != 8; ++i) std::printf("%04x", (unsigned)value->part[i]);
}

int main(int argc, char **argv)
{
  if (argc == 2 && std::strcmp(argv[1], "--abi") == 0) {
    std::printf("{\"integer_size\":%zu,\"integer_alignment\":%zu,"
                "\"part_size\":%zu,\"parts\":%zu,\"bits\":%zu,"
                "\"host_signed_size\":%zu,\"host_unsigned_size\":%zu,"
                "\"sizeof_t_size\":%zu,\"boolean_size\":%zu,"
                "\"char_bit\":%d,\"host_repr\":%d,\"fixed_point\":%d,"
                "\"checking\":%d}\n",
      sizeof(an_integer_value), alignof(an_integer_value), sizeof(an_int_value_part),
      (size_t)INT_VALUE_PARTS_PER_INTEGER_VALUE, (size_t)BITS_IN_AN_INTEGER_VALUE,
      sizeof(a_host_large_integer), sizeof(a_host_large_unsigned), sizeof(sizeof_t),
      sizeof(a_boolean), CHAR_BIT, INTEGER_VALUE_REPR_IS_A_HOST_INTEGER,
      FIXED_POINT_ALLOWED, CHECKING);
    return 0;
  }
  char operation[32], first[33], second[33];
  int alias, signed_1, signed_2, count;
  while (std::scanf("%31s %d %d %d %d %32s %32s", operation, &alias,
                   &signed_1, &signed_2, &count, first, second) == 7) {
    if (std::strlen(first) != 32 || std::strlen(second) != 32) return 2;
    an_integer_value a, b;
    read_parts(first, &a);
    read_parts(second, &b);
    an_integer_value *other = alias ? &a : &b;
    a_boolean err = false;
    a_host_large_integer host = 0;
    int comparison = 0;
    if (std::strcmp(operation, "add") == 0)
      add_integer_values(&a, other, signed_1 != 0, &err);
    else if (std::strcmp(operation, "add-mixed") == 0)
      add_mixed_signed_integer_values(&a, signed_1 != 0, other, signed_2 != 0, &err);
    else if (std::strcmp(operation, "subtract") == 0)
      subtract_integer_values(&a, other, signed_1 != 0, &err);
    else if (std::strcmp(operation, "subtract-mixed") == 0)
      subtract_mixed_signed_integer_values(&a, signed_1 != 0, other, signed_2 != 0, &err);
    else if (std::strcmp(operation, "left") == 0)
      shift_left_integer_value(&a, count, &err);
    else if (std::strcmp(operation, "right") == 0)
      shift_right_integer_value(&a, count, signed_1 != 0, signed_2 != 0);
    else if (std::strcmp(operation, "mask") == 0)
      make_integer_value_mask(&a, (size_t)count);
    else if (std::strcmp(operation, "extend") == 0)
      sign_extend_integer_value(&a, (size_t)count);
    else if (std::strcmp(operation, "negate") == 0)
      negate_integer_value(&a, &err);
    else if (std::strcmp(operation, "increment") == 0)
      incr_integer_value(&a);
    else if (std::strcmp(operation, "or") == 0)
      or_integer_values(&a, other);
    else if (std::strcmp(operation, "and") == 0)
      and_integer_values(&a, other);
    else if (std::strcmp(operation, "xor") == 0)
      xor_integer_values(&a, other);
    else if (std::strcmp(operation, "complement") == 0)
      complement_integer_value(&a);
    else if (std::strcmp(operation, "compare") == 0)
      comparison = cmp_integer_values(&a, signed_1 != 0, other, signed_2 != 0);
    else if (std::strcmp(operation, "extract") == 0)
      conv_integer_value_to_host_large_integer(&a, signed_1 != 0, &host, &err);
    else if (std::strcmp(operation, "set-signed") == 0) {
      a_host_large_unsigned raw = 0;
      for (int i = 4; i != 8; ++i) raw = (raw << 16) + other->part[i];
      std::memcpy(&host, &raw, sizeof(host));
      set_integer_value(&a, host);
      host = 0;
    } else if (std::strcmp(operation, "set-unsigned") == 0) {
      a_host_large_unsigned raw = 0;
      for (int i = 4; i != 8; ++i) raw = (raw << 16) + other->part[i];
      set_unsigned_integer_value(&a, raw);
    } else if (std::strcmp(operation, "copy-signed") == 0)
      set_integer_value(&a, *other);
    else if (std::strcmp(operation, "copy-unsigned") == 0)
      set_unsigned_integer_value(&a, *other);
    else return 3;
    print_parts(&a);
    std::putchar(' ');
    print_parts(other);
    std::printf(" %d %d %016llx %d\n", (int)err, comparison,
                (unsigned long long)(a_host_large_unsigned)host, (int)(other == &a));
  }
  return std::ferror(stdin) ? 4 : 0;
}
