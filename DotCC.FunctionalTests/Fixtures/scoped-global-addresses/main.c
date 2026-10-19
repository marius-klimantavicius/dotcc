#include <stdio.h>
#include <stdatomic.h>

struct Runtime {
    union { struct { int cells[3]; }; long alignment; };
    volatile int changed;
};
struct Runtime runtime = {{{{7, 11, 13}}}, 2};
struct Runtime other = {{{{17, 19, 23}}}, 5};
_Thread_local struct Runtime tls;
struct Runtime *volatile moving = &runtime;
_Atomic(struct Runtime *) atomic_base = &runtime;
int calls;

static void change(struct Runtime *value) { value->cells[0] += 3; }
static struct Runtime *next(void) { return ++calls == 1 ? &runtime : &other; }
static void tick(void) { ++calls; }

int safe(int branch) {
    int first = runtime.cells[0];
    runtime.cells[0] += 1;
    int second = runtime.cells[0];
    int pair = branch ? runtime.cells[1] + runtime.cells[2] : 0;
    change(&runtime);
    return first + second + pair + runtime.cells[0];
}
int branches(int branch) {
    if (branch) { return runtime.cells[0] + runtime.cells[1]; }
    return 0 && next()->cells[0];
}
int jump(int skip) {
    int value = 0;
    if (skip) goto finish;
    value = runtime.cells[0] + runtime.cells[1];
finish:
    return value;
}
int lazy(int branch) {
    int first = runtime.cells[0] + runtime.cells[1];
    int last = branch ? (tick(), runtime.cells[0]) : 0;
    return first + last;
}
int collect_check(int (*collect)(void)) {
    int first = runtime.cells[0];
    collect();
    return first + runtime.cells[0] + runtime.cells[1];
}
int main(void) {
    int stable = safe(1);
    struct Runtime *base = &runtime;
    int changing = base->cells[0];
    base = &other;
    changing += base->cells[0];
    calls = 0;
    int effects = next()->cells[0] + next()->cells[0];
    int branch = branches(0) + branches(1) + jump(1) + jump(0);
    int before = moving->cells[0];
    moving = &other;
    int after = moving->cells[0];
    int atomic_first = atomic_load(&atomic_base)->cells[0];
    atomic_store(&atomic_base, &other);
    int atomic_second = atomic_load(&atomic_base)->cells[0];
    tls.cells[0] = 29;
    int local = tls.cells[0];
    tls.cells[0] = 31;
    local += tls.cells[0];
    int qualified = runtime.changed;
    runtime.changed = 9;
    qualified += runtime.changed;
    int lazy_value = lazy(0) + lazy(1);
    printf("%d %d %d %d %d %d %d %d %d %d %d\n", stable, changing, effects, calls,
        branch, before, after, atomic_first + atomic_second, local, qualified, lazy_value);
    return 0;
}
