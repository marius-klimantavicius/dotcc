#include "program.h"

int main() {
    points_scalar();
    if (evaluations != 1) return 1;
    points_record();
    if (evaluations != 2) return 2;
    dot_scalar();
    if (evaluations != 3) return 3;
    dot_record();
    if (evaluations != 4) return 4;
    dot_field();
    if (evaluations != 5) return 5;
    dot_value();
    if (evaluations != 6) return 6;
    return 0;
}
