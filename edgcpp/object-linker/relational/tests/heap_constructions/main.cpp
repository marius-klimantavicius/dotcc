#include "program.h"

int main() {
    reset();
    Value *value = construct_value(false, false);
    if (value != reinterpret_cast<Value *>(arena) || value->self != value || value->value != 9 ||
        trace != 123 || allocation_calls != 1 || argument_calls != 1 || construction_calls != 1 ||
        deletion_calls || allocated_bytes != sizeof(Value)) return 1;
    delete value;
    if (trace != 12354 || deletion_calls != 1 || destruction_calls != 1 ||
        deleted_storage != arena || deleted_bytes != allocated_bytes) return 2;

    reset();
    try { construct_value(true, false); return 3; }
    catch (int error) { if (error != 22) return 4; }
    if (trace != 124 || construction_calls || deletion_calls != 1 ||
        deleted_storage != arena || deleted_bytes != allocated_bytes) return 5;

    reset();
    try { construct_value(false, true); return 6; }
    catch (int error) { if (error != 23) return 7; }
    if (trace != 1234 || construction_calls != 1 || destruction_calls || deletion_calls != 1 ||
        deleted_storage != arena || deleted_bytes != allocated_bytes) return 8;

    reset(); allocation_mode = 2;
    try { construct_value(false, false); return 9; }
    catch (int error) { if (error != 21) return 10; }
    if (trace != 1 || argument_calls || construction_calls || deletion_calls) return 11;

    reset(); allocation_mode = 1;
    if (construct_nullable(true) != nullptr || trace != 1 || argument_calls ||
        construction_calls || deletion_calls) return 12;

    reset();
    Nullable *nullable = construct_nullable(false);
    if (nullable != reinterpret_cast<Nullable *>(arena) || nullable->self != nullable ||
        nullable->value != 9 || trace != 123) return 13;
    delete nullable;
    if (trace != 1234 || deletion_calls != 1 || deleted_storage != arena) return 14;
    return 0;
}
