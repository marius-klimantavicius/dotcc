#include <stdio.h>

/* Reduced from Blink's SysSocketName(..., int SocketName(int, ...)).
 * Function-form parameters adjust to function pointers, including prototypes
 * paired with an explicitly pointer-form definition.
 */
struct record { int base; };
typedef unsigned extent_t;

static int apply(int callback(int), int value);
static int apply(int (*callback)(int), int value) { return callback(value); }
static int increment(int value) { return value + 1; }

static int inspect(int value, struct record *record, extent_t *size) {
  *size = 7;
  return value + record->base;
}
static int socket_form(int callback(int, struct record *, extent_t *), struct record *record) {
  extent_t size = 0;
  int value = callback(5, record, &size);
  return value + (int)size;
}

static int seven(void) { return 7; }
static int noargs(int callback()) { return callback(); }
static int notifications;
static void notify(void) { ++notifications; }
static void invoke(void callback(void)) { callback(); }

int main(void) {
  struct record record = {3};
  int scalar = apply(increment, 41);
  int socket = socket_form(inspect, &record);
  int empty = noargs(seven);
  invoke(notify);
  printf("scalar=%d socket=%d noargs=%d notifications=%d\n", scalar, socket, empty, notifications);
  return scalar == 42 && socket == 15 && empty == 7 && notifications == 1 ? 0 : 1;
}
