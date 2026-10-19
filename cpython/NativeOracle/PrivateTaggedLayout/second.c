#include <stddef.h>
struct Variant { long prefix; int value; unsigned char tail[]; };
static struct Variant second_state = {5,19};
int second(void) { return sizeof(struct Variant)==16 && offsetof(struct Variant,tail)==12
    && second_state.prefix==5 && second_state.value==19 && second_state.tail==(unsigned char *)&second_state+12; }
