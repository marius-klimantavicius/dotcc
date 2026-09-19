#include "shared.h"
struct table exported = { .format = "first", .entries = {{"alpha", 255, 7}, {"beta", 15, 9}, {0, 0, 0}} };
static struct table small = { "second", {{"only", 1, 11}, {0, 0, 0}} };
int read_static(void) { return small.entries[0].value + (small.entries[1].name == 0); }
