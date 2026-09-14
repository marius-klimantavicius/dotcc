#include <stdint.h>
#include <stdio.h>
struct Mode { uint8_t operand : 2, general : 6; };
struct Padding { unsigned char : 1, a : 2, : 1, b : 4; };
struct Signed { signed int a : 3, b : 5; };
int main(void) {
  struct Mode mode = {2, 1};
  struct Padding padding = {3, 9};
  struct Signed signed_value = {-2, -7};
  printf("mode=%u,%u size=%u byte=%u\n", mode.operand, mode.general,
         (unsigned)sizeof(mode), *(unsigned char *)&mode);
  printf("padding=%u,%u size=%u\n", padding.a, padding.b, (unsigned)sizeof(padding));
  printf("signed=%d,%d size=%u\n", signed_value.a, signed_value.b, (unsigned)sizeof(signed_value));
  mode.operand = 1; mode.general = 63;
  printf("updated=%u,%u byte=%u\n", mode.operand, mode.general, *(unsigned char *)&mode);
  return 0;
}
