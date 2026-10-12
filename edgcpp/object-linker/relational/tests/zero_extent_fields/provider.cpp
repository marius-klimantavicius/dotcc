#include "program.h"

Guarded guarded = {17, {9, {}, {'A', 'B', 'C'}}, 23};
Guarded *guarded_address() { return &guarded; }
unsigned long owner_size() { return sizeof(ShortShape); }
unsigned long padding_size() { return sizeof(((ShortShape *)0)->padding); }
char *padding_address(ShortShape *value) { return value->padding; }
char *dot_padding_address(ShortShape *value) { return (*value).padding; }
char *data_address(ShortShape *value) { return value->data; }

int local_check() {
    ShortShape local;
    local.header = 3;
    local.data[0] = 'L';
    local.data[22] = 'Z';
    if (padding_address(&local) != data_address(&local)) return 1;
    if (dot_padding_address(&local) != data_address(&local)) return 2;
    if (local.header != 3 || local.data[0] != 'L' || local.data[22] != 'Z') return 3;
    return 0;
}
