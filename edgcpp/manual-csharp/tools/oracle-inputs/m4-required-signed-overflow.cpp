// Fixed LP64 int is 32-bit; required evaluation cannot fold signed overflow.
static_assert((-2147483647 - 1) / -1 == 0);
