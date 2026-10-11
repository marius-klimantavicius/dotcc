#include "program.h"

int main() {
    Values values;
    values.small = -0.0f;
    values.wide = add_double(0.2);
    values.extended = add_extended(1.0L);
    values.quad = add_quad(1.0Q);
    if (!(reciprocal(values.small) < 0.0f)) return 1;
    if (values.wide != 0.30000000000000004) return 2;
    if (values.extended - 1.0L != 0x1p-63L) return 3;
    if (values.quad - 1.0Q != 0x1p-112Q) return 4;
    if (number<>() != 0.1) return 5;
    if (number<0.2>() != 0.2) return 6;
    return 0;
}
