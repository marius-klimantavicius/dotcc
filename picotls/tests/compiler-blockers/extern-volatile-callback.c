extern int (*volatile callback)(int);
static int increment(int value) { return value + 1; }
int (*volatile callback)(int) = increment;
int main(void) { return callback(41) - 42; }
