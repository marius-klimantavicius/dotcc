/* Real picotls header spells PTLS_THREADLOCAL as __thread on this target. */
typedef int value_t;
extern __thread value_t *current;
__thread value_t *current;
int main(void) { return current != 0; }
