typedef int (*callback)(int);
static int increment(int value) { return value + 7; }
static callback choose(void) { return increment; }
int main(void) {
    callback function = increment;
    callback *address = &function;
    void *opaque = address;
    if ((**(callback *)opaque)(11) != 18) return 1;
    if ((***address)(13) != 20) return 2;
    if ((*choose())(17) != 24) return 3;
    if ((&*function)(19) != 26) return 4;
    if ((&**address)(23) != 30) return 5;
    return 0;
}
