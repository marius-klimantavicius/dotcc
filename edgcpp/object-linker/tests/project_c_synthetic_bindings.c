struct Pair { int quotient; int remainder; };
static int calls;
static int next(void) { return ++calls; }
struct Pair divide(int value, int divisor) {
    return (struct Pair){ value / divisor, value % divisor };
}
int main(void) {
    struct Pair value = divide(37, 5);
    if (value.quotient != 7 || value.remainder != 2) return 1;
    struct Pair *pointer = &(struct Pair){ next(), next() };
    if (pointer->quotient != 1 || pointer->remainder != 2 || calls != 2) return 2;
    int *array = (int[]){ next(), next(), 0 };
    if (array[0] != 3 || array[1] != 4 || array[2] || calls != 4) return 3;
    if (((struct Pair){ 11, 13 }).remainder != 13) return 4;
    return 0;
}
