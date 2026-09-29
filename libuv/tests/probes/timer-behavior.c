#include "uv.h"
#include "uv-common.h"
#include <stdio.h>
static int trace[4], count, repeats;
static void fired(uv_timer_t* timer) {
  int id = (int)(uintptr_t)timer->data;
  trace[count++] = id;
  if (id == 3 && ++repeats == 2) uv_timer_stop(timer);
}
void behavior(void) {
  uv_loop_t loop = {0};
  uv_timer_t a = {0}, b = {0}, r = {0};
  loop.handle_queue.next = loop.handle_queue.prev = &loop.handle_queue;
  loop.time = 100;
  uv_timer_init(&loop, &a); uv_timer_init(&loop, &b); uv_timer_init(&loop, &r);
  printf("invalid=%d,%d\n", uv_timer_start(&a, NULL, 1, 0), uv_timer_again(&a));
  a.data = (void*)1; b.data = (void*)2; r.data = (void*)3;
  uv_timer_start(&a, fired, 5, 0); uv_timer_start(&b, fired, 5, 0); uv_timer_start(&r, fired, 3, 3);
  printf("initial=%d,%u\n", uv__next_timeout(&loop), loop.active_handles);
  loop.time = 103; uv__run_timers(&loop);
  loop.time = 105; uv__run_timers(&loop);
  loop.time = 106; uv__run_timers(&loop);
  printf("timers=%d,%d,%d,%d active=%u next=%d\n", trace[0], trace[1], trace[2], trace[3], loop.active_handles, uv__next_timeout(&loop));
}
