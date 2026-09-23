/* Valid, freestanding Linux x86-64 public-machine fixture. The same ELF runs
 * natively and through the translated interpreter. No fault/malformed cases. */
static long call(long n, long a, long b, long c, long d, long e, long f) {
  register long r10 __asm__("r10") = d;
  register long r8 __asm__("r8") = e;
  register long r9 __asm__("r9") = f;
  long result;
  __asm__ volatile("syscall" : "=a"(result) : "a"(n), "D"(a), "S"(b), "d"(c),
                   "r"(r10), "r"(r8), "r"(r9) : "rcx", "r11", "memory");
  return result;
}
static long length(const char *s) { long n = 0; while (s[n]) ++n; return n; }
static int equal(const char *a, const char *b) {
  while (*a && *a == *b) { ++a; ++b; } return *a == *b;
}
static int put(long fd, const char *s, long n) {
  while (n) { long k = call(1, fd, (long)s, n, 0, 0, 0); if (k <= 0) return 1; s += k; n -= k; }
  return 0;
}
static int text(long fd, const char *s) { return put(fd, s, length(s)); }
static int error(long value) {
  char digits[24]; long n = 0;
  if (value < 0) value = -value;
  do { digits[n++] = (char)('0' + value % 10); value /= 10; } while (value);
  text(2, "errno="); while (n) put(2, &digits[--n], 1); text(2, "\n"); return 1;
}
long guest_main(long *stack) {
  long argc = stack[0]; char **argv = (char **)(stack + 1);
  char **env = argv + argc + 1;
  if (argc < 2) return 2;
  if (equal(argv[1], "inspect")) {
    char cwd[512]; long result = call(79, (long)cwd, sizeof(cwd), 0, 0, 0, 0);
    if (result < 0) return error(result);
    text(1, "cwd="); text(1, cwd); text(1, "\n");
    for (long i = 0; i < argc; ++i) { text(1, "arg="); text(1, argv[i]); text(1, "\n"); }
    for (long i = 0; env[i]; ++i) { text(1, "env="); text(1, env[i]); text(1, "\n"); }
    text(2, "inspect-ok\n"); return 0;
  }
  if (equal(argv[1], "spin")) {
    text(1, "SPIN\n"); for (;;) __asm__ volatile("pause" ::: "memory");
  }
  if (equal(argv[1], "echo") || equal(argv[1], "wait")) {
    char buffer[37];
    if (equal(argv[1], "wait")) text(1, "WAIT\n");
    for (;;) {
      long n = call(0, 0, (long)buffer, sizeof(buffer), 0, 0, 0);
      if (n < 0) return error(n);
      if (!n) break;
      if (put(1, buffer, n)) return 3;
    }
    text(2, "echo-eof\n"); return 0;
  }
  if (argc < 3) return 2;
  if (equal(argv[1], "mkdir")) {
    long result = call(83, (long)argv[2], 0755, 0, 0, 0, 0);
    if (result < 0) return error(result);
    text(1, "mkdir-ok\n"); return 0;
  }
  if (equal(argv[1], "rename")) {
    if (argc != 4) return 2;
    long result = call(82, (long)argv[2], (long)argv[3], 0, 0, 0, 0);
    if (result < 0) return error(result);
    text(1, "rename-ok\n"); return 0;
  }
  int write = equal(argv[1], "store") || equal(argv[1], "append") || equal(argv[1], "store-spin");
  if (!write && !equal(argv[1], "load")) return 2;
  if (write && argc != 4) return 2;
  long flags = write ? 1 | 64 | (equal(argv[1], "append") ? 1024 : 512) : 0;
  long fd = call(257, -100, (long)argv[2], flags, 0600, 0, 0);
  if (fd < 0) return error(fd);
  if (write) {
    if (text(fd, argv[3])) return 3;
  } else {
    char buffer[37];
    for (;;) {
      long n = call(0, fd, (long)buffer, sizeof(buffer), 0, 0, 0);
      if (n < 0) return error(n);
      if (!n) break;
      if (put(1, buffer, n)) return 3;
    }
  }
  long result = call(3, fd, 0, 0, 0, 0, 0);
  if (result < 0) return error(result);
  if (write) text(1, "store-ok\n");
  if (equal(argv[1], "store-spin")) { text(1, "SPIN\n"); for (;;) __asm__ volatile("pause" ::: "memory"); }
  return 0;
}
__asm__(".global _start\n_start:\nmov %rsp,%rdi\nand $-16,%rsp\ncall guest_main\nmov %rax,%rdi\nmov $60,%eax\nsyscall\n");
