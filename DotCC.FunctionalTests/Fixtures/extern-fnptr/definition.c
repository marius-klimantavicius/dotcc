static int increment(int value) { return value + 1; }
static int twice(int value) { return value * 2; }
int (*volatile callback)(int) = increment;
int (*const table[])(int) = {increment, twice};
