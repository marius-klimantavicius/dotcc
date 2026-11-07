/* Freestanding Linux x86-64: see mapping-signal-fixture.md for the schedule,
 * native receipts and the limits of a timing-based lock-cycle witness. */
#include <stddef.h>
#include <stdint.h>

#ifndef RESTART_CONTROL
#define RESTART_CONTROL 0
#endif
#ifndef DEFAULT_EXPECT_BLOCKED
#define DEFAULT_EXPECT_BLOCKED 0
#endif

enum { SIGUSR1 = 10, EINTR = 4, PAGE = 4096, STACK = 65536 };
enum { SA_RESTORER = 0x04000000, SA_RESTART = 0x10000000 };
enum { REPORT = 1, EXPECT_CLEAR = 2, EXPECT_BLOCKED = 3 };

static long sys6(long number, long a, long b, long c, long d, long e, long f) {
  register long r10 __asm__("r10") = d;
  register long r8 __asm__("r8") = e;
  register long r9 __asm__("r9") = f;
  __asm__ volatile("syscall" : "+a"(number)
                   : "D"(a), "S"(b), "d"(c), "r"(r10), "r"(r8), "r"(r9)
                   : "rcx", "r11", "memory");
  return number;
}

static __attribute__((noreturn)) void fail(int code) {
  for (;;) sys6(231, code, 0, 0, 0, 0, 0);
}
#define CHECK(test, code) do { if (!(test)) fail(code); } while (0)

/* Aligned 32-bit loads/stores are acquire/release publications on x86-64.
 * Compiler atomics keep the C memory model valid without a runtime library. */
static unsigned load(const unsigned *word) {
  return __atomic_load_n(word, __ATOMIC_ACQUIRE);
}
static void publish(unsigned *word, unsigned value) {
  __atomic_store_n(word, value, __ATOMIC_RELEASE);
}

/* Kept as an ELF symbol for optional debugger/RAM inspection. blocked is only
 * !writer_done sampled by C before tkill, never proof of an internal mutex. */
struct mapping_signal_observation {
  unsigned read_announced, writer_ready, sender_ready, writer_started;
  unsigned writer_done, sender_done, handler_signal, blocked;
  long writer_result, signal_result, read_result;
};
struct mapping_signal_observation mapping_signal_report;
static unsigned writer_tid, sender_tid;
static unsigned char writer_stack[STACK] __attribute__((aligned(PAGE)));
static unsigned char sender_stack[STACK] __attribute__((aligned(PAGE)));
static unsigned char *read_buffer, *unmap_buffer;
static int pipe_fds[2];
static long reader_tid;

extern long mapping_signal_clone(void (*function)(void), void *stack_top,
                                 unsigned *clear_tid);
extern void mapping_signal_restorer(void);

struct kernel_action {
  void (*handler)(int);
  unsigned long flags;
  void (*restorer)(void);
  unsigned long mask;
};
struct kernel_timespec { long seconds, nanoseconds; };

static void sleep_ns(long ns) {
  struct kernel_timespec delay = {0, ns}, remaining;
  for (unsigned retry = 0; retry < 8; ++retry) {
    long rc = sys6(35, (long)&delay, (long)&remaining, 0, 0, 0, 0);
    if (!rc) return;
    CHECK(rc == -EINTR, 30);
    delay = remaining;
  }
  fail(31);
}

static void wait_value(const unsigned *word, unsigned value, int code) {
  for (unsigned poll = 0; poll < 2000; ++poll) {
    if (load(word) == value) return;
    sleep_ns(1000000);
  }
  fail(code);
}

/* No mapping operation, syscall, nonlocal exit or context edit in the handler.
 * Its ordinary RET reaches the raw rt_sigreturn restorer in mapping-signal.S. */
static void handler(int signal) {
  publish(&mapping_signal_report.handler_signal, (unsigned)signal);
}

static void writer(void) {
  publish(&mapping_signal_report.writer_ready, 1);
  wait_value(&mapping_signal_report.read_announced, 1, 32);
  sleep_ns(20000000);  /* Give A time to enter read; this is not an entry hook. */
  publish(&mapping_signal_report.writer_started, 1);
  long rc = sys6(11, (long)unmap_buffer, PAGE, 0, 0, 0, 0);
  mapping_signal_report.writer_result = rc;
  CHECK(rc == 0, 33);
  publish(&mapping_signal_report.writer_done, 1);
}

static void sender(void) {
  publish(&mapping_signal_report.sender_ready, 1);
  wait_value(&mapping_signal_report.writer_started, 1, 34);
  sleep_ns(100000000);  /* Opportunity for B to reach its page-lock wait. */
  publish(&mapping_signal_report.blocked,
          load(&mapping_signal_report.writer_done) == 0);
  long rc = sys6(200, reader_tid, SIGUSR1, 0, 0, 0, 0);
  mapping_signal_report.signal_result = rc;
  CHECK(rc == 0, 35);
#if RESTART_CONTROL
  wait_value(&mapping_signal_report.handler_signal, SIGUSR1, 36);
  static const unsigned char byte = 0x5a;
  CHECK(sys6(1, pipe_fds[1], (long)&byte, 1, 0, 0, 0) == 1, 37);
#endif
  publish(&mapping_signal_report.sender_done, 1);
}

static int equal(const char *a, const char *b) {
  while (*a && *a == *b) { ++a; ++b; }
  return *a == *b;
}

static void write_all(int fd, const char *text, size_t size) {
  while (size) {
    long rc = sys6(1, fd, (long)text, (long)size, 0, 0, 0);
    if (rc == -EINTR) continue;
    CHECK(rc > 0 && (unsigned long)rc <= size, 38);
    text += rc;
    size -= (size_t)rc;
  }
}

static unsigned char *anonymous_page(void) {
  long address = sys6(9, 0, PAGE, 3, 0x22, -1, 0);
  CHECK((unsigned long)address < (unsigned long)-4095L, 13);
  unsigned char *page = (void *)address;
  *(volatile unsigned char *)page = 0;  /* Materialize before starting B/C. */
  return page;
}

int mapping_signal_main(long argc, char **argv) {
  int option = DEFAULT_EXPECT_BLOCKED ? EXPECT_BLOCKED : 0;
  CHECK(argc == 1 || argc == 2, 10);
  if (argc == 2) {
    if (equal(argv[1], "--report")) option = REPORT;
    else if (equal(argv[1], "--expect-blocked=0")) option = EXPECT_CLEAR;
    else if (equal(argv[1], "--expect-blocked=1")) option = EXPECT_BLOCKED;
    else fail(10);
  }
  struct kernel_action action = {
    handler, SA_RESTORER | (RESTART_CONTROL ? SA_RESTART : 0),
    mapping_signal_restorer, 0
  };
  CHECK(sys6(13, SIGUSR1, (long)&action, 0, 8, 0, 0) == 0, 11);
  unsigned long empty_mask = 0;
  CHECK(sys6(14, 2, (long)&empty_mask, 0, 8, 0, 0) == 0, 12);
  read_buffer = anonymous_page();
#if RESTART_CONTROL
  unmap_buffer = anonymous_page();  /* Native restarted read needs valid RAM. */
#else
  unmap_buffer = read_buffer;
#endif
  CHECK(sys6(22, (long)pipe_fds, 0, 0, 0, 0, 0) == 0, 14);
  reader_tid = sys6(186, 0, 0, 0, 0, 0, 0);
  CHECK(reader_tid > 0, 15);
  /* ELF BSS metadata and both stacks are disjoint from the anonymous buffer.
   * First-touch every stack page before either clone; no allocator or TLS. */
  for (unsigned offset = 0; offset < STACK; offset += PAGE) {
    *(volatile unsigned char *)(writer_stack + offset) = 0;
    *(volatile unsigned char *)(sender_stack + offset) = 0;
  }
  CHECK(mapping_signal_clone(writer, writer_stack + STACK, &writer_tid) > 0, 16);
  CHECK(mapping_signal_clone(sender, sender_stack + STACK, &sender_tid) > 0, 17);
  wait_value(&mapping_signal_report.writer_ready, 1, 18);
  wait_value(&mapping_signal_report.sender_ready, 1, 19);

  publish(&mapping_signal_report.read_announced, 1);
  long rc = sys6(0, pipe_fds[0], (long)read_buffer, 1, 0, 0, 0);
  mapping_signal_report.read_result = rc;
  CHECK(load(&mapping_signal_report.handler_signal) == SIGUSR1, 20);
#if RESTART_CONTROL
  CHECK(rc == 1 && *read_buffer == 0x5a, 21);
#else
  CHECK(rc == -EINTR, 21);  /* Outer cleanup must release its PageLocks. */
#endif
  wait_value(&mapping_signal_report.writer_done, 1, 22);
  wait_value(&mapping_signal_report.sender_done, 1, 23);
  /* Also wait for CLONE_CHILD_CLEARTID, so success publishes after both exits. */
  wait_value(&writer_tid, 0, 24);
  wait_value(&sender_tid, 0, 25);
  CHECK(mapping_signal_report.writer_result == 0 &&
        mapping_signal_report.signal_result == 0, 26);
  CHECK(sys6(3, pipe_fds[0], 0, 0, 0, 0, 0) == 0, 27);
  CHECK(sys6(3, pipe_fds[1], 0, 0, 0, 0, 0) == 0, 27);
#if RESTART_CONTROL
  CHECK(sys6(11, (long)read_buffer, PAGE, 0, 0, 0, 0) == 0, 28);
#endif
  unsigned blocked = load(&mapping_signal_report.blocked);
  if (option) {
    char report[] = "mapping-signal: blocked=0\n";
    report[sizeof(report) - 3] = (char)('0' + blocked);
    write_all(2, report, sizeof(report) - 1);
  }
  if (option == EXPECT_CLEAR) CHECK(blocked == 0, 40);
  if (option == EXPECT_BLOCKED) CHECK(blocked == 1, 41);
#if RESTART_CONTROL
  static const char success[] = "mapping-signal: SA_RESTART read recovery ok\n";
#else
  static const char success[] = "mapping-signal: recursive read/munmap recovery ok\n";
#endif
  write_all(1, success, sizeof(success) - 1);
  return 0;
}
