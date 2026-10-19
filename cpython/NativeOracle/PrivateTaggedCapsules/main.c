#include "codec.h"
void *first(void); void *second(void);
int first_size(void); int second_size(void);
int main(void) {
    Codec *a=first(), *b=second();
    return first_size()==16 && second_size()==24 && a!=b && a==a->self && b==b->self
        && a->state==a->slots[0] && b->state==b->slots[1] && a->read!=b->read
        && a->read(a->state)==7 && b->read(b->state)==42 ? 0:1;
}
