#include <stdio.h>
static int effects;
static long next_count(void) { ++effects; return 3; }
int main(void) {
  unsigned long long value = 0x123456789ULL;
  long left = 8;
  unsigned long right = 4;
  unsigned long long slots[2] = {7, 11};
  int index = 0;
  value <<= left;
  value >>= right;
  slots[index++] <<= next_count();
  slots[index++] >>= (effects++, (long)1);
  printf("%llu %llu %llu %d %d\n", value, slots[0], slots[1], index, effects);
  return 0;
}
