#include <stdio.h>
#include "shared.h"
int first(void);
int second(void);
int main(void) {
    Packet *p = packet();
    if (p != packet() || p->callback != callback || inspect(p) != 42 || p->callback(p->value.node) != 42) return 2;
    printf("%d %d %d %d\n", first(), second(), inspect(p), (int)sizeof(Packet));
    return 0;
}
