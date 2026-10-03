#include <threads.h>
#include <stdio.h>
#include <stdlib.h>
static thrd_t initial, parent, expected_last;
static tss_t key;
static int mode;
static void require(int ok) { if (!ok) _Exit(88); }
static void emit(int ch) { fputc(ch, stderr); }
#ifdef __cplusplus
struct Local { int mark; ~Local() { emit(mark); } };
struct Tls { int mark; ~Tls() { if (mark) emit(mark); } };
static thread_local Tls tls;
static Local global = {'G'};
#endif
static void destructor(void *p) {
  emit(p == (void *)1 ? 'M' : 'W');
  if (p == (void *)1 && mode == 3) thrd_exit(41);
}
static void final(void) { require(thrd_equal(expected_last, thrd_current())); emit('A'); }
static int child(void *p) {
  int result; (void)p;
  require(thrd_join(parent, &result) == thrd_success && result == 77);
  expected_last = thrd_current(); emit('C'); return 99;
}
static int worker(void *p) {
  int result; (void)p;
#ifdef __cplusplus
  Local automatic = {'X'}; tls.mark = 'U';
#endif
  require(thrd_join(initial, &result) == thrd_success && result == (mode == 3 ? 41 : 37));
  emit('J'); expected_last = thrd_current();
  require(tss_set(key, (void *)2) == thrd_success);
  if (mode == 2) { thrd_t descendant; parent = thrd_current(); require(thrd_create(&descendant, child, 0) == thrd_success); }
  return 77;
}
int main(int argc, char **argv) {
  require(argc == 2); mode = argv[1][0] - '0';
#ifdef __cplusplus
  Local automatic = {'D'}; tls.mark = 'T';
#endif
  initial = expected_last = thrd_current();
  require(atexit(final) == 0 && tss_create(&key, destructor) == thrd_success && tss_set(key, (void *)1) == thrd_success);
  if (mode != 0 && mode != 5) { thrd_t other; require(thrd_create(&other, worker, 0) == thrd_success); if (mode == 4) require(thrd_detach(other) == thrd_success); }
  if (mode == 5) require(thrd_detach(initial) == thrd_success);
  thrd_exit(37);
}
