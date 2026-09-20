#include <stdatomic.h>
#include <stdio.h>
int main(void) {
  _Atomic unsigned char byte = 3;
  _Atomic unsigned short word = 7;
  unsigned char expected_byte = 3;
  unsigned short expected_word = 8;
  atomic_compare_exchange_strong(&byte, &expected_byte, 11);
  atomic_compare_exchange_weak_explicit(&word, &expected_word, 13,
                                      memory_order_seq_cst, memory_order_seq_cst);
  atomic_flag flag = ATOMIC_FLAG_INIT;
  atomic_flag_test_and_set(&flag);
  int previous = atomic_flag_test_and_set(&flag);
  printf("atomic=%u,%u expected=%u,%u flag=%d\n", (unsigned)byte,
         (unsigned)word, (unsigned)expected_byte, (unsigned)expected_word, previous);
  return byte != 11 || word != 7 || expected_byte != 3 || expected_word != 7 || previous != 1;
}
