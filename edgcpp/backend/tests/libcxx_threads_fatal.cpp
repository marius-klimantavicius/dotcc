#include <__edg_thread_abi.h>
#include <stdio.h>
struct Guard { ~Guard() { puts("unwind"); fflush(stdout); } };
static void* fail(void*) {
  Guard guard;
  puts("body");
  fflush(stdout);
  throw 19;
}
extern "C" int thread_fatal() {
  unsigned long handle = 0;
  if (__edg_libcpp_thread_create(&handle, fail, 0)) return 1;
  __edg_libcpp_thread_join(&handle);
  return 2;
}
