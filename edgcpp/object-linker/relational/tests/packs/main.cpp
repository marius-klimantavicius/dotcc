#include "program.h"

int main() {
    if (pack_result() != 16) return 1;
    if (count() != 0) return 2;
    if (sum<4, 7>() != 11) return 3;
    return 0;
}
