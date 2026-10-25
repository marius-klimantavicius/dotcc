#define _GNU_SOURCE 1
#include <stdint.h>
#include <sys/mman.h>
#include <unistd.h>

#define PAGE 4096
#define CHECK(x) do { if (!(x)) { result = __LINE__; goto Cleanup; } } while (0)
int main(void) {
  const uintptr_t boundaries[] = {0x300000000000ull, 0x300040000000ull,
                                 0x300000200000ull};
  unsigned char *held = MAP_FAILED, *selected = MAP_FAILED;
  int result = 0;
  for (unsigned n = 0; n < sizeof(boundaries) / sizeof(*boundaries); ++n) {
    held = mmap((void *)boundaries[n], PAGE, PROT_READ | PROT_WRITE,
                MAP_PRIVATE | MAP_ANONYMOUS | MAP_FIXED_NOREPLACE, -1, 0);
    CHECK(held == (void *)boundaries[n]);
    held[19] = 127 + n;
    selected = mmap((void *)(boundaries[n] - PAGE * 3 + 123), PAGE * 6 - 17,
                    PROT_READ | PROT_WRITE, MAP_PRIVATE | MAP_ANONYMOUS, -1, 0);
    CHECK(selected != MAP_FAILED);
    CHECK(((uintptr_t)selected & (PAGE - 1)) == 0);
    CHECK((uintptr_t)selected + PAGE * 6 <= (uintptr_t)held ||
          (uintptr_t)selected >= (uintptr_t)held + PAGE);
    selected[17] = 99;
    CHECK(held[19] == 127 + n);
    CHECK(!munmap(selected, PAGE * 6)); selected = MAP_FAILED;
    CHECK(!munmap(held, PAGE)); held = MAP_FAILED;
  }
Cleanup:
  if (selected != MAP_FAILED && munmap(selected, PAGE * 6) && !result) result = __LINE__;
  if (held != MAP_FAILED && munmap(held, PAGE) && !result) result = __LINE__;
  if (!result && write(1, "mapping hints passed\n", 21) != 21) result = __LINE__;
  return result;
}
