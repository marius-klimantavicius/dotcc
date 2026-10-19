#include <stddef.h>
struct Variant { int value; unsigned char tail[]; };
static struct Variant first_state = {7};
int first(void) { return sizeof(struct Variant)==4 && offsetof(struct Variant,tail)==4
    && first_state.value==7 && first_state.tail==(unsigned char *)&first_state+4; }
