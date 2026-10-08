static int value;
static void reset() { value = 17; }
static int *address() { return &value; }
static int read_value() { return value; }
int main(void) {
    reset();
    if (*address() != 17 || read_value() != 17) return 1;
    *address() = 23;
    if (read_value() != 23) return 2;
    return 0;
}
