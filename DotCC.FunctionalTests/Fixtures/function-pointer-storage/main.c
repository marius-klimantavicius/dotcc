#include <stdio.h>
typedef int (*Callback)(int);
typedef void (*Notification)(void);
struct Slots { int (**handlers)(int); void (**notify)(void); };
static int notifications;
static void notify(void) { ++notifications; }
static int twice(int x) { return x * 2; }
static int increment(int x) { return x + 1; }
int main(void) {
    Callback storage[2] = {twice, increment};
    struct Slots slots;
    Notification sink = notify;
    void (**notification)(void) = &sink;
    void (**legacy)();
    int (**view)(int) = storage;
    slots.handlers = view;
    view = (int (**)(int))slots.handlers;
    slots.notify = notification;
    legacy = (void (**)())notification;
    notification = (void (**)(void))legacy;
    (*notification)();
    (*slots.notify)();
    printf("%d %d %d\n", view[0](21), slots.handlers[1](16), notifications);
    return 0;
}
