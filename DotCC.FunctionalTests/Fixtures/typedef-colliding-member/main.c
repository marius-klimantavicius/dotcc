#include <stdio.h>
typedef long decimal;
typedef struct Number { decimal value; } Number;
typedef struct Holder {
    union { Number decimal; long integer; } data;
} Holder;
struct Separate { Number decimal; };
int main(void) {
    Holder h;
    struct Separate s;
    Holder *p = &h;
    decimal number = 41;
    p->data.decimal.value = number;
    s.decimal.value = 1;
    {
        Number *decimal = &s.decimal;
        decimal->value = 1;
    }
    decimal restored = h.data.decimal.value;
    printf("%ld\n", restored + s.decimal.value);
    return 0;
}
