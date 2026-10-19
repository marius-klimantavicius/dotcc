#include <stdio.h>
#include <string.h>

struct State { int value; } first = {7}, second = {19};
struct State *slot = &first;
int tally = 3;
static const _Bool falsehood = 0;

static int read_slot(struct State **address) { return (*address)->value; }
static void store_slot(struct State **address, struct State *value) { *address = value; }
static int read_int(int *address) { return *address; }

int run(int (*callback)(void)) {
    int before = read_slot(&slot);
    callback();
    store_slot(&slot, &second);
    return before + read_slot(&slot);
}
int tally_run(void) {
    int before = read_int(&tally);
    tally += 5;
    return before + read_int(&tally);
}
int current_value(void) { return read_slot(&slot); }
static int bool_cmp(const _Bool *left, const _Bool *right) {
    return (memcmp(left, &falsehood, sizeof(_Bool)) != 0) ==
           (memcmp(right, &falsehood, sizeof(_Bool)) != 0);
}
int bool_run(void) {
    _Bool yes = 1, no = 0;
    return bool_cmp(&yes, &no) + 2 * bool_cmp(&no, &no);
}
static int native_callback(void) { return 0; }
int main(void) {
    int value = run(native_callback);
    int changed = tally_run();
    printf("%d %d %d %d\n", value, changed, current_value(), bool_run());
    return 0;
}
