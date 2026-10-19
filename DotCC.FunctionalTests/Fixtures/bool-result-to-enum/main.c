#include <stdio.h>
enum Choice { No, Yes };
static enum Choice returned(int value) { return value > 0; }
static int argument(enum Choice value) { return value; }
int main(void) {
    int x = 3, y = 1, counter = 0;
    _Bool boolean = 9;
    enum Choice explicit_cast = (enum Choice)(x && y);
    enum Choice implicit_store = x < y;
    enum Choice comma = (enum Choice)(counter++, x > y);
    enum Choice normalized = (enum Choice)boolean;
    implicit_store = x > y;
    printf("%d %d %d %d %d %d %d\n", explicit_cast, implicit_store,
           comma, normalized, counter, returned(0), argument(x == 3));
    return 0;
}
