#include <stdint.h>
#include <stdio.h>
#include <string.h>

typedef uint8_t u8;
typedef int64_t i64;
struct Machine { unsigned char bytes[64]; unsigned writes; };
static int CopyToUserWrite(struct Machine *m, i64 address, const void *data,
                           size_t size) {
  if (address < 0 || (uint64_t)address + size > sizeof(m->bytes)) return -1;
  memcpy(m->bytes + address, data, size);
  ++m->writes;
  return 0;
}

#include "getcpu-handler.inc"

int main(void) {
  struct Machine m;
  const unsigned char zero[4] = {0};
  for (unsigned requested = 0; requested < 4; ++requested) {
    memset(&m, 0xa5, sizeof(m));
    m.writes = 0;
    if (SysGetcpu(&m, requested & 1 ? 8 : 0, requested & 2 ? 24 : 0, 40)) return 1;
    if (m.writes != (requested & 1) + ((requested >> 1) & 1)) return 2;
    for (unsigned index = 0; index < sizeof(m.bytes); ++index) {
      int written = ((requested & 1) && index >= 8 && index < 12) ||
                    ((requested & 2) && index >= 24 && index < 28);
      if (m.bytes[index] != (written ? zero[0] : 0xa5)) return 3;
    }
  }
  puts("guest-getcpu-boundary: nullable outputs and exact 32-bit guest writes: PASS");
  return 0;
}
