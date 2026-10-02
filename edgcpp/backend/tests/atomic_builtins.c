/* Shared synthetic C/C++ input: no dependency on host atomic headers. */
#ifdef __cplusplus
extern "C" {
#endif
static unsigned long counter;
void atomic_increment(void) { __atomic_fetch_add(&counter, 1UL, __ATOMIC_RELAXED); }
unsigned long atomic_counter(void) { return __atomic_load_n(&counter, __ATOMIC_SEQ_CST); }

#define CHECK_WIDTH(T) do { \
  T n = 0, expected = 9; \
  __atomic_store_n(&n, (T)7, __ATOMIC_RELEASE); \
  if (__atomic_load_n(&n, __ATOMIC_ACQUIRE) != 7) return __LINE__; \
  if (__atomic_compare_exchange_n(&n, &expected, (T)15, 1, __ATOMIC_ACQ_REL, __ATOMIC_ACQUIRE) || expected != 7) return __LINE__; \
  if (!__atomic_compare_exchange_n(&n, &expected, (T)15, 0, __ATOMIC_SEQ_CST, __ATOMIC_SEQ_CST) || expected != 7) return __LINE__; \
  if (__atomic_exchange_n(&n, (T)20, __ATOMIC_SEQ_CST) != 15) return __LINE__; \
  if (__atomic_fetch_add(&n, (T)3, __ATOMIC_RELAXED) != 20 || n != 23) return __LINE__; \
  if (__atomic_add_fetch(&n, (T)4, __ATOMIC_CONSUME) != 27) return __LINE__; \
  if (__atomic_fetch_sub(&n, (T)5, __ATOMIC_ACQUIRE) != 27 || n != 22) return __LINE__; \
  if (__atomic_sub_fetch(&n, (T)6, __ATOMIC_RELEASE) != 16) return __LINE__; \
  if (__atomic_fetch_or(&n, (T)3, __ATOMIC_ACQ_REL) != 16 || n != 19) return __LINE__; \
  if (__atomic_or_fetch(&n, (T)12, __ATOMIC_SEQ_CST) != 31) return __LINE__; \
  if (__atomic_fetch_and(&n, (T)23, __ATOMIC_RELAXED) != 31 || n != 23) return __LINE__; \
  if (__atomic_and_fetch(&n, (T)15, __ATOMIC_SEQ_CST) != 7) return __LINE__; \
  if (__atomic_fetch_xor(&n, (T)12, __ATOMIC_SEQ_CST) != 7 || n != 11) return __LINE__; \
  if (__atomic_xor_fetch(&n, (T)15, __ATOMIC_SEQ_CST) != 4) return __LINE__; \
  if (__atomic_fetch_nand(&n, (T)7, __ATOMIC_SEQ_CST) != 4 || n != (T)~(T)4) return __LINE__; \
  if (__atomic_nand_fetch(&n, (T)255, __ATOMIC_SEQ_CST) != (T)~(T)((T)~(T)4 & (T)255)) return __LINE__; \
  n = (T)-1; if (__atomic_add_fetch(&n, (T)1, __ATOMIC_SEQ_CST) != 0) return __LINE__; \
} while (0)

#define CHECK_OBJECT(N) do { \
  struct Obj { unsigned char bytes[N]; }; \
  struct Obj n, desired, expected, result; \
  int i; for (i=0; i<N; ++i) desired.bytes[i]=(unsigned char)(i+7); \
  __atomic_store(&n, &desired, __ATOMIC_RELEASE); \
  __atomic_load(&n, &expected, __ATOMIC_ACQUIRE); \
  for (i=0; i<N; ++i) if (expected.bytes[i]!=(unsigned char)(i+7)) return __LINE__; \
  for (i=0; i<N; ++i) desired.bytes[i]=(unsigned char)(i+9); \
  if (!__atomic_compare_exchange(&n,&expected,&desired,0,__ATOMIC_SEQ_CST,__ATOMIC_SEQ_CST)) return __LINE__; \
  if (__atomic_compare_exchange(&n,&expected,&desired,1,__ATOMIC_SEQ_CST,__ATOMIC_SEQ_CST)) return __LINE__; \
  for (i=0; i<N; ++i) if (expected.bytes[i]!=(unsigned char)(i+9)) return __LINE__; \
  for (i=0; i<N; ++i) desired.bytes[i]=(unsigned char)(i+11); \
  __atomic_exchange(&n,&desired,&result,__ATOMIC_SEQ_CST); \
  for (i=0; i<N; ++i) if (result.bytes[i]!=(unsigned char)(i+9) || n.bytes[i]!=(unsigned char)(i+11)) return __LINE__; \
} while (0)

int atomic_cases(void) {
  CHECK_WIDTH(unsigned char);
  CHECK_WIDTH(unsigned short);
  CHECK_WIDTH(unsigned int);
  CHECK_WIDTH(unsigned long);
  CHECK_WIDTH(unsigned __int128);
  CHECK_OBJECT(3);
  CHECK_OBJECT(16);
  CHECK_OBJECT(600);
  unsigned char flag=0;
  if (__atomic_test_and_set(&flag,__ATOMIC_ACQUIRE)) return __LINE__;
  if (!__atomic_test_and_set(&flag,__ATOMIC_SEQ_CST)) return __LINE__;
  __atomic_clear(&flag,__ATOMIC_RELEASE);
  if (flag) return __LINE__;
  __atomic_thread_fence(__ATOMIC_SEQ_CST);
  __atomic_signal_fence(__ATOMIC_SEQ_CST);
  __sync_synchronize();
  return 0;
}
#ifdef __cplusplus
}
#endif
#ifdef ATOMIC_ORACLE_MAIN
int main(void) { return atomic_cases(); }
#endif
