#include <stdio.h>

struct Op { unsigned long bits; };
struct Cache {
  unsigned long address;
  unsigned char *page;
  union { unsigned size; unsigned alias; };
  struct Op op;
};
struct Machine { struct Cache cache[1]; };

unsigned char *update(struct Machine *m, unsigned char *data, unsigned long pc, unsigned n) {
  m->cache->address = pc;
  m->cache->page = data;
  m->cache->size = n;
  m->cache->op.bits = ~0UL;
  return m->cache->page + (pc & 3);
}
unsigned read_size(const struct Machine *m) { return m->cache->size; }
long signed_value(long value) { return value; }

int main(void) {
  struct Machine m = {0};
  unsigned char bytes[4] = {10, 20, 30, 40};
  unsigned char *p = update(&m, bytes, 3, 2);
  printf("%u %lu %u %lu %ld\n", *p, m.cache[0].address, read_size(&m),
         m.cache->address & ~1, signed_value(m.cache->op.bits));
  m.cache->page = 0;
  return m.cache[0].page != 0 || m.cache[0].alias != 2;
}
