#include <stdatomic.h>
#include <stdio.h>
#define CHECK(condition) do { if (!(condition)) { printf("FAIL atomic data line=%d\n", __LINE__); return 2; } } while (0)
static int orders;
static int next_order(int order) { ++orders; return order; }
enum Phase { PhaseStart = 3, PhaseEnd = 7 };
int main(void) {
 atomic_int value = 10;
 CHECK(atomic_load_explicit(&value, next_order(memory_order_acquire)) == 10);
 atomic_store_explicit(&value, 12, next_order(memory_order_release));
 CHECK(atomic_exchange_explicit(&value, 20, next_order(memory_order_acq_rel)) == 12);
 int expected = 20;
 CHECK(atomic_compare_exchange_strong_explicit(&value, &expected, 25, next_order(memory_order_acq_rel), next_order(memory_order_acquire)));
 CHECK(expected == 20 && value == 25);
 expected = 20;
 CHECK(!atomic_compare_exchange_strong_explicit(&value, &expected, 30, next_order(memory_order_seq_cst), next_order(memory_order_acquire)));
 CHECK(expected == 25 && value == 25);
 expected = 0;
 CHECK(!atomic_compare_exchange_weak_explicit(&value, &expected, 30, next_order(memory_order_seq_cst), next_order(memory_order_acquire)));
 CHECK(expected == 25 && value == 25);
 CHECK(atomic_fetch_add_explicit(&value, 5, next_order(memory_order_relaxed)) == 25);
 CHECK(atomic_fetch_sub_explicit(&value, 3, next_order(memory_order_acq_rel)) == 30);
 CHECK(atomic_fetch_or_explicit(&value, 32, next_order(memory_order_release)) == 27);
 CHECK(atomic_fetch_and_explicit(&value, 31, next_order(memory_order_acquire)) == 59);
 CHECK(atomic_fetch_xor_explicit(&value, 3, next_order(memory_order_seq_cst)) == 27);
 CHECK(value == 24);
 atomic_flag flag = ATOMIC_FLAG_INIT;
 CHECK(!atomic_flag_test_and_set_explicit(&flag, next_order(memory_order_acquire)));
 CHECK(atomic_flag_test_and_set_explicit(&flag, next_order(memory_order_acq_rel)));
 atomic_flag_clear_explicit(&flag, next_order(memory_order_release));
 atomic_thread_fence(next_order(memory_order_acq_rel));
 atomic_signal_fence(next_order(memory_order_relaxed));
 atomic_bool boolean = 0;
 CHECK(!atomic_load_explicit(&boolean, next_order(memory_order_relaxed)));
 atomic_store_explicit(&boolean, 1, next_order(memory_order_release));
 CHECK(boolean);
 _Atomic(enum Phase) phase = PhaseStart;
 CHECK(atomic_load_explicit(&phase, next_order(memory_order_acquire)) == PhaseStart);
 atomic_store_explicit(&phase, PhaseEnd, next_order(memory_order_release));
 CHECK(phase == PhaseEnd);
 int loops = 0;
 while (atomic_load_explicit(&value, next_order(memory_order_acquire)) == 24 && loops < 2) ++loops;
 CHECK(loops == 2);
 int unused = 0 ? atomic_load_explicit(&value, next_order(memory_order_acquire)) : 7;
 CHECK(unused == 7);
 if (0 && atomic_load_explicit(&value, next_order(memory_order_acquire))) return 3;
 printf("atomic-order evaluations=%d expected=26 data=PASS\n", orders);
 return orders == 26 ? 0 : 1;
}
