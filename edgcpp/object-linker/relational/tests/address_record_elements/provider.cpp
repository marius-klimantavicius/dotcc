#include "program.h"
Entry entries[2] = {{First, "alpha"}, {Second, "beta"}};
Holder holder = {11, 23, {31, 37, 41}};
int receivers = 0;
int indices = 0;
Holder* selected_holder() { ++receivers; return &holder; }
volatile int* selected_values() { ++receivers; return holder.values; }
int selected_index() { ++indices; return 1; }
int receiver_calls() { return receivers; }
int index_calls() { return indices; }
void reset_calls() { receivers = 0; indices = 0; }
Entry* entry_address(int index) { return &entries[index]; }
int inspect_entries() {
    if (entries[0].kind != First || entries[1].kind != Second) return 1;
    if (entries[0].text[0] != 'a' || entries[0].text[4] != 'a' || entries[0].text[5] != 0) return 2;
    if (entries[1].text[0] != 'b' || entries[1].text[3] != 'a' || entries[1].text[4] != 0) return 3;
    for (int index = 6; index < 48; ++index) if (entries[0].text[index] != 0) return 4;
    for (int index = 5; index < 48; ++index) if (entries[1].text[index] != 0) return 5;
    return 0;
}
