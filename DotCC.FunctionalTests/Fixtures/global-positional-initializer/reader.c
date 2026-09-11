#include "shared.h"
int read_clock(void) { return shared_clock.cb() + shared_clock.omitted; }
