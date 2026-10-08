#include "storage_fixture.h"

int counter = 7;
int *counter_pointer = &counter;
static int values[] = { 3, 5, 8, 13 };
static int *tail = values + 2;
struct Entry operations[2] = {
    { "increment", increment, 3 },
    { "again", increment, 5 },
};
union Word word = { 0x04030201U };
struct Flags flags = { 19, -7, 1 };
__thread int thread_counter = 11;
struct Opaque { int value; };
static struct Opaque opaque = { 31 };

int increment(int value) { return value + *counter_pointer; }
int run_operation(int index, int value) {
    return operations[index].apply(value) + operations[index].bias + *tail;
}
int static_counter(void) { static int count = 2; return ++count; }
struct Opaque *opaque_instance(void) { return &opaque; }
int opaque_read(struct Opaque *value) { return value->value; }
