#include "program.h"
int main() {
    int counter = 0;
    int total = 0;
    int i;
    for (i = 0; i < 3; ++i) total += provider(i, &counter);
    if (total != 12 || counter != 6) return 1;
    if (selected_address(5) != 10) return 2;
    return 0;
}
