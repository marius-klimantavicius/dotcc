#include "program.h"
int counted(int *count) { ++*count; return 7; }
int main() {
    int value = 42;
    int count = 0;
    short second = 9;
    float fraction = 2.5f;
    if (scan(5, counted(&count), second, fraction, &value, true)) return 1;
    if (count != 1) return 2;
    Scanner target = select_scan();
    if (target(5, 7, second, fraction, &value, true)) return 3;
    if (empty(17) != 17) return 4;
    int cleanup = 0;
    try { raises(&cleanup, 0, 13); return 5; }
    catch (int error) { if (error != 13 || cleanup != 1) return 6; }
    return 0;
}
