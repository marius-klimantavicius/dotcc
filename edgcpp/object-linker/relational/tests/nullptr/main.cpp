#include "program.h"
int main() {
    Null local = make_null();
    Holder holder;
    holder.value = local;
    holder.pointer = as_pointer(holder.value);
    if (holder.pointer != nullptr) return 1;
    if (accept_null(local) != 3) return 2;
    if (select(nullptr) != 5) return 3;
    if (select(holder.pointer) != 4) return 4;
    if (null_template<>() != 7 || null_template<nullptr>() != 7) return 5;
    if (sizeof(Null) != sizeof(void *)) return 6;
    int counter = 0;
    int *effect = with_effect(&counter);
    if (effect != nullptr || counter != 1) return 7;
    Null stored = nullptr;
    unsigned char *bytes = reinterpret_cast<unsigned char *>(&stored);
    bytes[0] = 255;
    int *normalized = stored;
    if (normalized != nullptr) return 8;
    return 0;
}
