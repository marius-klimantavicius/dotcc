#include <stdio.h>

unsigned char byte_value(_Bool value) { return value; }
unsigned short short_value(_Bool value) { return value; }
unsigned int_value(_Bool value) { return value; }
unsigned long long_value(_Bool value) { return value; }
unsigned __int128 wide_value(_Bool value) { return value; }
float float_value(_Bool value) { return value; }
double double_value(_Bool value) { return value; }
unsigned consume(unsigned value) { return value; }
unsigned long consume_long(unsigned long value) { return value; }
void store(unsigned *target, _Bool value) { *target = value; }

int main(void) {
  _Bool yes = -7, no = 0;
  unsigned stored = 0;
  struct Values { unsigned small; unsigned long large; } values = {yes, yes};
  store(&stored, yes);
  if (consume(yes) != 1 || consume_long(no) != 0 || values.small != 1 || values.large != 1) return 1;
  printf("%u %u %u %lu %lu %.1f %.1f %u\n", byte_value(yes), short_value(yes),
         int_value(yes), long_value(yes), (unsigned long)wide_value(yes),
         float_value(no), double_value(yes), stored);
  return 0;
}
