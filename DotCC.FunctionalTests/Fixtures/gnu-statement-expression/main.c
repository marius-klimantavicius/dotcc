#include <stdio.h>
#include <stddef.h>
#define typeof __typeof__
#define container_of(ptr, type, member) ({ const typeof(((type *)0)->member) *__mptr = (ptr); (type *)((char *)__mptr - offsetof(type, member)); })
struct entry { int before; int value; };
static int calls;
static int *once(struct entry *e) { ++calls; return &e->value; }
int main(void) {
    enum { FIRST = 4, SECOND };
    int value = 3;
    int result = ({ int value = SECOND; value += 2; value; });
    int skipped = 0 && ({ ++calls; 1; });
    int selected = 1 ? ({ ++calls; 9; }) : ({ calls += 100; 0; });
    struct entry e = { 11, 22 };
    struct entry *p = container_of(once(&e), struct entry, value);
    int loop = 0;
    while (({ ++loop; loop < 3; })) { }
    printf("%d %d %d %d %d %d %d\n", value, result, skipped, selected, p->before, calls, loop);
    return 0;
}
