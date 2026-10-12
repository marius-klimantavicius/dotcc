#include "program.h"
int main() {
    if (inspect_entries() != 0) return 1;
    reset_calls();
    volatile int* field = field_address();
    if (field != &holder.field || *field != 23 || receiver_calls() != 1 || index_calls() != 0) return 2;
    volatile int* indexed = indexed_address();
    if (indexed != &holder.values[1] || *indexed != 37 || receiver_calls() != 2 || index_calls() != 1) return 3;
    *field = 47;
    *indexed = 53;
    if (holder.canary != 11 || holder.field != 47 || holder.values[0] != 31 || holder.values[1] != 53 || holder.values[2] != 41) return 4;
    if (reinterpret_cast<char*>(entry_address(1)) - reinterpret_cast<char*>(entry_address(0)) != sizeof(Entry)) return 5;
    return 0;
}
