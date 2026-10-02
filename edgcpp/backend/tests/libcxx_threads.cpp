// Synthetic translated calls to the same ABI used by libc++ external threading.
#include <__edg_thread_abi.h>
#include <stdlib.h>
#include <errno.h>
static unsigned long mutex, condition, once_flag, key;
static int count, initialized, key_destructions, tls_destructions, failures;
static int released, allow_exit;
static int* retained_report;
thread_local int stage;
struct ThreadObject {
  ThreadObject() { stage = 1; }
  ~ThreadObject() { stage = 2; __atomic_fetch_add(&tls_destructions, 1, __ATOMIC_SEQ_CST); }
};
thread_local ThreadObject object;
static void initialize() { ++initialized; }
static void key_destructor(void* value) {
  if (retained_report) __atomic_fetch_or(retained_report, stage == 2 && errno == 37 ? 1 : 256, __ATOMIC_SEQ_CST);
  if (stage != 2 || errno != 37) __atomic_fetch_add(&failures, 1, __ATOMIC_SEQ_CST);
  free(value);
  __atomic_fetch_add(&key_destructions, 1, __ATOMIC_SEQ_CST);
}
static void prepare_thread() {
  (void)&object;
  errno = 37;
  int* p = (int*)malloc(sizeof(int));
  if (!p) { __atomic_fetch_add(&failures, 1, __ATOMIC_SEQ_CST); return; }
  *p = 19;
  if (__edg_libcpp_tls_set(key, p) != 0 || __edg_libcpp_tls_get(key) != p)
    __atomic_fetch_add(&failures, 1, __ATOMIC_SEQ_CST);
}
static void* increment(void*) {
  prepare_thread();
  if (__edg_libcpp_execute_once(&once_flag, initialize) != 0) return 0;
  for (int i = 0; i != 1000; ++i) {
    if (__edg_libcpp_mutex_lock(&mutex, 0) != 0) return 0;
    ++count;
    if (__edg_libcpp_mutex_unlock(&mutex, 0) != 0) return 0;
  }
  return 0;
}
static void* notify(void*) {
  if (__edg_libcpp_mutex_lock(&mutex, 0) != 0) return 0;
  released = 1;
  __edg_libcpp_condvar_signal(&condition, 0);
  __edg_libcpp_mutex_unlock(&mutex, 0);
  return 0;
}
static void* retained(void*) {
  prepare_thread();
  while (!__atomic_load_n(&allow_exit, __ATOMIC_ACQUIRE)) __edg_libcpp_thread_yield();
  return 0;
}
static void* created_by_global_destructor(void*) {
  prepare_thread();
  __atomic_fetch_or(retained_report, 2, __ATOMIC_SEQ_CST);
  return 0;
}
struct ShutdownSpawner {
  ~ShutdownSpawner() {
    if (retained_report) {
      unsigned long handle = 0;
      if (__edg_libcpp_thread_create(&handle, created_by_global_destructor, 0) != 0 ||
          __edg_libcpp_thread_detach(&handle) != 0)
        __atomic_fetch_or(retained_report, 512, __ATOMIC_SEQ_CST);
    }
  }
};
static ShutdownSpawner shutdown_spawner;
extern "C" int thread_cases() {
  unsigned long handles[4] = {};
  if (__edg_libcpp_tls_create(&key, key_destructor) != 0) return 1;
  for (int i = 0; i != 4; ++i) if (__edg_libcpp_thread_create(&handles[i], increment, 0) != 0) return 2;
  for (int i = 0; i != 4; ++i) {
    if (!__edg_libcpp_thread_get_id(&handles[i]) || __edg_libcpp_thread_join(&handles[i]) != 0) return 3;
  }
  if (count != 4000 || initialized != 1 || failures || key_destructions != 4 || tls_destructions != 4) return 4;
  if (__edg_libcpp_mutex_lock(&mutex, 0) != 0) return 5;
  if (__edg_libcpp_thread_create(&handles[0], notify, 0) != 0) return 6;
  while (!released) if (__edg_libcpp_condvar_wait(&condition, &mutex, 0) != 0) return 7;
  struct timespec past = { 0, 0 };
  if (__edg_libcpp_condvar_wait(&condition, &mutex, &past) != ETIMEDOUT) return 8;
  if (__edg_libcpp_mutex_unlock(&mutex, 0) != 0 || __edg_libcpp_thread_join(&handles[0]) != 0) return 9;
  if (__edg_libcpp_condvar_destroy(&condition) != 0 || __edg_libcpp_mutex_destroy(&mutex, 0) != 0) return 10;
  return 0;
}
extern "C" int thread_count() { return count; }
extern "C" int thread_diagnostics(int which) {
  return which == 0 ? count : which == 1 ? initialized : which == 2 ? failures :
         which == 3 ? key_destructions : tls_destructions;
}
extern "C" int* start_retained_thread(int* report) {
  unsigned long handle = 0;
  retained_report = report;
  if (__edg_libcpp_thread_create(&handle, retained, 0) != 0 || __edg_libcpp_thread_detach(&handle) != 0) return 0;
  return &allow_exit;
}
