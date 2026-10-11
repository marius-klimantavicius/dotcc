#include "program.h"
int main() {
    if (provider_defaults()) return 1;
    Defaults first;
    Defaults second(41);
    if (first.value != 7 || second.value != 41 || first.pointer || second.pointer) return 2;
    Number<19> selected;
    return selected.value == 19 ? 0 : 3;
}
