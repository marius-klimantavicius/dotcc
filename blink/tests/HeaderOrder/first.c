#include <sys/socket.h>
#include "blink/linux.h"
#include <stddef.h>
#include <stdio.h>
int First(void) {
  struct linger host = {1, 13};
  struct linger_linux guest = {{1, 0, 0, 0}, {7, 0, 0, 0}};
  printf("first host=%zu/%zu/%zu/%zu guest=%zu/%zu/%zu/%zu values=%d/%u\n",
         sizeof(host), _Alignof(struct linger), offsetof(struct linger, l_onoff),
         offsetof(struct linger, l_linger), sizeof(guest), _Alignof(struct linger_linux),
         offsetof(struct linger_linux, onoff), offsetof(struct linger_linux, linger),
         host.l_linger, guest.linger[0]);
  struct { char lead; struct linger value; } placed_host;
  struct { char lead; struct linger_linux value; } placed_guest;
  printf("first placement=%td/%td fields=%td/%td\n",
         (char *)&placed_host.value - (char *)&placed_host,
         (char *)&placed_guest.value - (char *)&placed_guest,
         (char *)&host.l_linger - (char *)&host,
         (char *)&guest.linger - (char *)&guest);
  return host.l_linger != 13 || guest.linger[0] != 7;
}
