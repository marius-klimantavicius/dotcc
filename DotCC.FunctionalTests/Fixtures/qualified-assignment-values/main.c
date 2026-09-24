#include <stdio.h>
#include <stdatomic.h>

enum Phase { ZERO=0, READY=7, DONE=9 };
static _Atomic enum Phase phase;
static volatile unsigned char slots[3];
static int addresses, values;
static volatile unsigned char *slot(void) { addresses++; return &slots[1]; }
static int value(void) { values++; return 259; }
static int numbers[4] = {10,20,30,40};
static int *volatile cursor = numbers;

int main(void) {
    volatile unsigned char a=0,b=0;
    int chain = (a = b = value());
    int sum = (*slot() += 5);
    int old = (*slot())++;
    int now = ++(*slot());
    int shifted = (*slot() <<= 2);
    volatile int n=-4;
    int divided = (n /= 3UL);
    int *before = cursor++;
    int *after = ++cursor;
    int *assigned = (cursor = numbers + 1);
    int *compound = (cursor += 2);
    cursor = numbers;
    cursor++;
    --cursor;
    int steps = 0;
    for (; cursor != numbers + 4; cursor++) steps++;
    if (steps != 4) return 2;
    atomic_store(&phase, READY);
    int initial = atomic_load(&phase);
    enum Phase observed = atomic_exchange(&phase, DONE);
    enum Phase expected = DONE;
    int changed = atomic_compare_exchange_strong(&phase, &expected, READY);
    int final = atomic_load_explicit(&phase, memory_order_acquire);
    _Atomic double fraction;
    atomic_init(&fraction, 3.75);
    double precise = atomic_load(&fraction);
    printf("%d %d %d %d %d %d %d %d %d %d %d %d %d %d %d %d %.2f\n",
        chain,a,b,sum,old,now,shifted,addresses,values,divided,
        *before,*after,*assigned,*compound,initial,observed,precise);
    return changed != 1 || final != 7;
}
