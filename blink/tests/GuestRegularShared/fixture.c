/* Ordinary Linux file mapping operations. All paths are guest-local, or files
 * in the native oracle's private working directory. No injected faults. */
typedef unsigned long u64;
static long call6(long n, long a, long b, long c, long d, long e, long f) {
  register long r10 __asm__("r10") = d, r8 __asm__("r8") = e, r9 __asm__("r9") = f;
  long result;
  __asm__ volatile("syscall" : "=a"(result) : "a"(n), "D"(a), "S"(b), "d"(c),
      "r"(r10), "r"(r8), "r"(r9) : "rcx", "r11", "memory");
  return result;
}
#define SC(n,a,b,c,d,e,f) call6(n,(long)(a),(long)(b),(long)(c),(long)(d),(long)(e),(long)(f))
static __attribute__((noreturn)) void finish(int status) {
  SC(231, status, 0, 0, 0, 0, 0); __builtin_unreachable();
}
#define CHECK(x) do { if (!(x)) finish(__LINE__); } while (0)
static unsigned char *map(long length, int prot, int flags, int fd, long offset) {
  long result = SC(9, 0, length, prot, flags, fd, offset);
  CHECK((u64)result < (u64)-4095);
  return (unsigned char *)result;
}
static void put(int fd, long offset, unsigned char value) {
  CHECK(SC(18, fd, &value, 1, offset, 0, 0) == 1);
}
static void code(int fd, unsigned char value) {
  unsigned char bytes[] = {0xb8, value, 0, 0, 0, 0xc3};
  CHECK(SC(18, fd, bytes, sizeof(bytes), 0, 0, 0) == sizeof(bytes));
}
void _start(void) {
  int fd = SC(2, "regular-a", 2 | 64 | 128, 0600, 0, 0, 0);
  CHECK(fd >= 0);
  CHECK(SC(77, fd, 8196, 0, 0, 0, 0) == 0);
  code(fd, 42); put(fd, 4101, 71); put(fd, 8195, 72);
  int reader = SC(2, "regular-a", 0, 0, 0, 0, 0);
  CHECK(reader >= 0);
  int duplicate = SC(32, reader, 0, 0, 0, 0, 0);
  CHECK(duplicate >= 0);
  volatile unsigned char *a = map(12288, 5, 1, reader, 0);
  volatile unsigned char *b = map(12288, 1, 1, duplicate, 0);
  unsigned char *snapshot = map(4096, 3, 2, reader, 4096);
  CHECK(a[4101] == 71 && b[8195] == 72 && a[8196] == 0);
  snapshot[6] = 99; /* Force ordinary Linux COW before changing the file. */
  CHECK(((int (*)(void))a)() == 42);
  code(fd, 43); put(fd, 4101, 73);
  CHECK(((int (*)(void))a)() == 43 && a[4101] == 73 && b[4101] == 73 && snapshot[5] == 71);
  CHECK(SC(8, fd, 4102, 0, 0, 0, 0) == 4102);
  unsigned char value = 74;
  CHECK(SC(1, fd, &value, 1, 0, 0, 0) == 1 && b[4102] == 74);
  int append = SC(2, "regular-a", 1 | 1024, 0, 0, 0, 0);
  CHECK(append >= 0);
  value = 75;
  CHECK(SC(1, append, &value, 1, 0, 0, 0) == 1 && a[8196] == 75);
  put(append, 0, 76); /* Linux O_APPEND also applies to pwrite. */
  CHECK(b[8197] == 76 && a[0] == 0xb8);
  volatile unsigned char *tail = map(4096, 1, 1, reader, 16384);
  CHECK(SC(77, fd, 20480, 0, 0, 0, 0) == 0 && tail[0] == 0);
  put(fd, 16384, 77); CHECK(tail[0] == 77);
  CHECK(SC(77, fd, 8193, 0, 0, 0, 0) == 0);
  CHECK(SC(77, fd, 20480, 0, 0, 0, 0) == 0);
  CHECK(a[8195] == 0 && b[8197] == 0 && tail[0] == 0);
  CHECK(SC(76, "regular-a", 0, 0, 0, 0, 0) == 0);
  CHECK(SC(76, "regular-a", 8196, 0, 0, 0, 0) == 0);
  code(fd, 44); CHECK(((int (*)(void))a)() == 44);
  int truncated = SC(2, "regular-a", 1 | 512, 0, 0, 0, 0);
  CHECK(truncated >= 0 && SC(3, truncated, 0, 0, 0, 0, 0) == 0);
  CHECK(SC(76, "regular-a", 8196, 0, 0, 0, 0) == 0);
  code(fd, 44); CHECK(((int (*)(void))a)() == 44);
  CHECK(SC(10, (void *)b, 4096, 3, 0, 0, 0) == -13);
  CHECK(SC(9, 0, 4096, 3, 1, reader, 0) == -13);
  CHECK(a[0] == 0xb8 && b[0] == 0xb8);
  CHECK(SC(82, "regular-a", "regular-moved", 0, 0, 0, 0) == 0);
  int replacement = SC(2, "regular-a", 2 | 64 | 128, 0600, 0, 0, 0);
  CHECK(replacement >= 0);
  CHECK(SC(77, replacement, 4096, 0, 0, 0, 0) == 0);
  put(replacement, 0, 91);
  volatile unsigned char *other = map(4096, 1, 1, replacement, 0);
  CHECK(other[0] == 91 && a[0] == 0xb8);
  CHECK(SC(82, "regular-a", "regular-moved", 0, 0, 0, 0) == 0); /* replace original name */
  code(fd, 45); CHECK(((int (*)(void))a)() == 45 && other[0] == 91);
  CHECK(SC(87, "regular-moved", 0, 0, 0, 0, 0) == 0);
  CHECK(SC(3, fd, 0, 0, 0, 0, 0) == 0);
  int reused = SC(2, "regular-reused", 2 | 64 | 128, 0600, 0, 0, 0);
  CHECK(reused == fd);
  CHECK(SC(3, reader, 0, 0, 0, 0, 0) == 0);
  CHECK(SC(3, duplicate, 0, 0, 0, 0, 0) == 0);
  CHECK(SC(3, append, 0, 0, 0, 0, 0) == 0);
  CHECK(SC(3, replacement, 0, 0, 0, 0, 0) == 0);
  CHECK(((int (*)(void))a)() == 45 && other[0] == 91);
  CHECK(SC(11, (void *)a, 12288, 0, 0, 0, 0) == 0);
  CHECK(SC(11, (void *)b, 12288, 0, 0, 0, 0) == 0);
  CHECK(SC(11, (void *)tail, 4096, 0, 0, 0, 0) == 0);
  CHECK(SC(11, snapshot, 4096, 0, 0, 0, 0) == 0);
  CHECK(SC(11, (void *)other, 4096, 0, 0, 0, 0) == 0);
  CHECK(SC(3, reused, 0, 0, 0, 0, 0) == 0);
  CHECK(SC(87, "regular-reused", 0, 0, 0, 0, 0) == 0);
  static const char message[] = "guest-regular-shared: coherence/identity/private/EOF/permissions: PASS\n";
  CHECK(SC(1, 1, message, sizeof(message) - 1, 0, 0, 0) == sizeof(message) - 1);
  finish(0);
}
