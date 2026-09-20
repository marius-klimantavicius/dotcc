#include "host-epoll.h"
#include <stdio.h>
int main(void) {
  printf("private layout %zu %zu %zu\n", sizeof(struct blink_host_epoll_event),
         _Alignof(struct blink_host_epoll_event), offsetof(struct blink_host_epoll_event, data));
  return 0;
}
