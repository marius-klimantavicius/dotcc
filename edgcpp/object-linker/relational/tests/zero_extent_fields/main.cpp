#include "program.h"

int main() {
    Guarded *global = guarded_address();
    if (owner_size() != 24 || sizeof(Guarded) != 26 || padding_size() != 0) return 1;
    if (padding_address(&global->value) - (char *)&global->value != 1) return 2;
    if (padding_address(&global->value) != data_address(&global->value)) return 3;
    if (dot_padding_address(&global->value) != data_address(&global->value)) return 4;
    if (global->before != 17 || global->after != 23 || global->value.header != 9) return 5;
    if (global->value.data[0] != 'A' || global->value.data[1] != 'B' || global->value.data[2] != 'C') return 6;
    for (int i = 3; i < 23; ++i) if (global->value.data[i] != 0) return 7;
    global->value.data[0] = 'R';
    global->value.data[22] = 'S';
    if (global->before != 17 || global->after != 23 || global->value.header != 9) return 8;
    if (global->value.data[0] != 'R' || global->value.data[22] != 'S') return 9;
    global->value.data[0] = 'A';
    global->value.data[22] = 0;
    return local_check() == 0 ? 0 : 10;
}
