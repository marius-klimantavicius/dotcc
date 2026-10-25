/* Normal x86-64 Linux memfd/double-map workload. No libc, injected faults,
 * native execution of guest memory by Blink, or dependency on host file paths. */
typedef unsigned long u64;
static long call6(long number, long a, long b, long c, long d, long e, long f) {
  register long r10 __asm__("r10") = d;
  register long r8 __asm__("r8") = e;
  register long r9 __asm__("r9") = f;
  long result;
  __asm__ volatile("syscall" : "=a"(result) : "a"(number), "D"(a), "S"(b),
      "d"(c), "r"(r10), "r"(r8), "r"(r9) : "rcx", "r11", "memory");
  return result;
}
#define SC(n,a,b,c,d,e,f) call6(n,(long)(a),(long)(b),(long)(c),(long)(d),(long)(e),(long)(f))
static __attribute__((noreturn)) void finish(int status) {
  SC(231, status, 0, 0, 0, 0, 0);
  __builtin_unreachable();
}
#define CHECK(x) do { if (!(x)) finish(__LINE__); } while (0)
static void *map(void *at, long size, int prot, int flags, int fd, long offset) {
  long result = SC(9, at, size, prot, flags, fd, offset);
  CHECK((u64)result < (u64)-4095);
  return (void *)result;
}
static void code(volatile unsigned char *at, unsigned char value) {
  at[0] = 0xb8; at[1] = value; at[2] = at[3] = at[4] = 0; at[5] = 0xc3;
}
void _start(void) {
  const long huge = 2L * 1024 * 1024 * 1024 * 1024;
  int fd = SC(319, "doublemapper-fixture", 1, 0, 0, 0, 0);
  CHECK(fd >= 0);
  CHECK(SC(72, fd, 1, 0, 0, 0, 0) == 1); /* FD_CLOEXEC */
  CHECK(SC(77, fd, huge, 0, 0, 0, 0) == 0);
  CHECK(SC(8, fd, 0, 2, 0, 0, 0) == huge);
  int duplicate = SC(32, fd, 0, 0, 0, 0, 0);
  CHECK(duplicate >= 0 && duplicate != fd);
  CHECK(SC(72, duplicate, 1, 0, 0, 0, 0) == 0);
  CHECK(SC(8, duplicate, 0, 1, 0, 0, 0) == huge); /* shared position */
  unsigned char *rx = map(0, 12288, 0, 34, -1, 0);
  CHECK(map(rx, 12288, 0, 17, fd, 0) == rx); /* fixed shared reservation */
  CHECK(SC(10, rx, 8192, 5, 0, 0, 0) == 0);
  volatile unsigned char *rw = map(0, 12288, 3, 1, duplicate, 0);
  code(rw, 42); code(rw + 4096, 73);
  CHECK(((int (*)(void))rx)() == 42);
  CHECK(((int (*)(void))(rx + 4096))() == 73);
  code(rw, 43);
  CHECK(((int (*)(void))rx)() == 43);
  rw[8192] = 91;
  unsigned char *snapshot = map(0, 4096, 3, 2, fd, 8192);
  CHECK(snapshot[0] == 91);
  snapshot[1] = 1; /* Ordinary COW makes Linux's private view independent. */
  rw[8192] = 92;
  CHECK(snapshot[0] == 91); /* private file behavior remains a snapshot */
  unsigned char byte = 0;
  CHECK(SC(17, duplicate, &byte, 1, 8192, 0, 0) == 1 && byte == 92);
  byte = 93;
  CHECK(SC(18, fd, &byte, 1, 8192, 0, 0) == 1 && rw[8192] == 93);
  CHECK(SC(11, rx + 4096, 4096, 0, 0, 0, 0) == 0);
  CHECK(SC(10, rx, 4096, 0, 0, 0, 0) == 0);
  CHECK(SC(10, rx, 4096, 5, 0, 0, 0) == 0);
  CHECK(((int (*)(void))rx)() == 43);
  CHECK(SC(11, (void *)(rw + 4096), 4096, 0, 0, 0, 0) == 0);
  CHECK(map((void *)(rw + 4096), 4096, 3, 17, duplicate, 4096) == (void *)(rw + 4096));
  CHECK(rw[4097] == 73);
  volatile unsigned char *distant = map(0, 4096, 3, 1, duplicate, 1L << 32);
  distant[0] = 77;
  CHECK(SC(17, duplicate, &byte, 1, 1L << 32, 0, 0) == 1 && byte == 77);
  CHECK(SC(77, fd, 0, 0, 0, 0, 0) == 0);
  CHECK(SC(77, fd, huge, 0, 0, 0, 0) == 0);
  CHECK(rw[1] == 0 && distant[0] == 0);
  code(rw, 43); distant[0] = 77;
  CHECK(((int (*)(void))rx)() == 43); /* Re-fetch after normal shrink/regrow. */
  CHECK(SC(3, fd, 0, 0, 0, 0, 0) == 0);
  int reused = SC(319, "independent", 0, 0, 0, 0, 0);
  CHECK(reused == fd);
  CHECK(SC(77, reused, 4096, 0, 0, 0, 0) == 0);
  volatile unsigned char *other = map(0, 4096, 3, 1, reused, 0);
  CHECK(other[0] == 0);
  other[0] = 99;
  CHECK(rw[0] == 0xb8);
  CHECK(SC(3, duplicate, 0, 0, 0, 0, 0) == 0);
  CHECK(SC(3, reused, 0, 0, 0, 0, 0) == 0);
  code(rw, 44); /* mapping leases outlive all descriptors */
  CHECK(((int (*)(void))rx)() == 44 && distant[0] == 77 && other[0] == 99);
  CHECK(SC(11, rx, 12288, 0, 0, 0, 0) == 0);
  CHECK(SC(11, (void *)rw, 12288, 0, 0, 0, 0) == 0);
  CHECK(SC(11, snapshot, 4096, 0, 0, 0, 0) == 0);
  CHECK(SC(11, (void *)distant, 4096, 0, 0, 0, 0) == 0);
  CHECK(SC(11, (void *)other, 4096, 0, 0, 0, 0) == 0);
  CHECK(SC(1, 1, "guest memfd aliases passed\n", 26, 0, 0, 0) == 26);
  finish(0);
}
