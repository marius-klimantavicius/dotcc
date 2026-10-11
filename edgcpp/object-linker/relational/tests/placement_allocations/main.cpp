#include "program.h"

bool identity(Value *result) {
    return result == (Value *)arena && result->self == result && result->stable() && result->value == 9;
}
bool counts(long long expected, int placement, int allocation, int arguments, int constructor, int deletion, int destruction, int live) {
    return trace == expected && placement_calls == placement && allocation_calls == allocation &&
        constructor_arguments == arguments && constructor_calls == constructor && deletion_calls == deletion &&
        destructions == destruction && alive == live;
}

int main() {
    reset();
    Value *first = construct_pointer(false, false, false);
    if (!identity(first) || allocated_bytes != sizeof(Value) || !counts(1234, 1, 1, 1, 1, 0, 0, 1)) return 1;
    first->~Value();
    if (!counts(12346, 1, 1, 1, 1, 0, 1, 0)) return 2;
    reset();
    try { construct_pointer(false, false, true); return 3; }
    catch (int &failure) { if (failure != 37) return 4; }
    if (!counts(12345, 1, 1, 1, 1, 1, 0, 0) || deleted_result != (void *)arena ||
        deleted_argument != (void *)arena || current_pointer != (void *)other_arena) return 5;
    reset();
    try { construct_pointer(false, true, false); return 6; }
    catch (int &failure) { if (failure != 29) return 7; }
    if (!counts(1235, 1, 1, 1, 0, 1, 0, 0) || deleted_result != (void *)arena || deleted_argument != (void *)arena) return 8;
    reset();
    try { construct_pointer(true, false, false); return 9; }
    catch (int &failure) { if (failure != 23) return 10; }
    if (!counts(1, 1, 0, 0, 0, 0, 0, 0)) return 11;

    reset();
    Value *second = construct_tag(false, false);
    if (!identity(second) || allocated_tag != 7 || !counts(1234, 1, 1, 1, 1, 0, 0, 1)) return 12;
    second->~Value();
    if (!counts(12346, 1, 1, 1, 1, 0, 1, 0)) return 13;
    reset();
    return_null = true;
    if (construct_tag(false, false) != nullptr || allocated_tag != 7 || tag_value != 7 ||
        !counts(12, 1, 1, 0, 0, 0, 0, 0)) return 14;
    reset();
    try { construct_tag(false, true); return 15; }
    catch (int &failure) { if (failure != 37) return 16; }
    if (!counts(12345, 1, 1, 1, 1, 1, 0, 0) || deleted_result != (void *)arena ||
        deleted_tag != 7 || tag_value != 99) return 17;
    reset();
    try { construct_tag(true, false); return 18; }
    catch (int &failure) { if (failure != 29) return 19; }
    if (!counts(1235, 1, 1, 1, 0, 1, 0, 0) || deleted_tag != 7 || tag_value != 99) return 20;

    reset();
    Value *third = construct_throwing(false, false);
    if (!identity(third) || allocated_tag != 7 || !counts(1234, 1, 1, 1, 1, 0, 0, 1)) return 21;
    third->~Value();
    if (!counts(12346, 1, 1, 1, 1, 0, 1, 0)) return 22;
    reset();
    allocator_failure = true;
    try { construct_throwing(false, false); return 23; }
    catch (int &failure) { if (failure != 41) return 24; }
    if (!counts(12, 1, 1, 0, 0, 0, 0, 0)) return 25;
    reset();
    try { construct_throwing(false, true); return 26; }
    catch (int &failure) { if (failure != 37) return 27; }
    if (!counts(12345, 1, 1, 1, 1, 1, 0, 0) || deleted_tag != 7) return 28;

    reset();
    construct_discard(false, false);
    Value *discarded = (Value *)arena;
    if (!identity(discarded) || !counts(1234, 1, 1, 1, 1, 0, 0, 1)) return 29;
    discarded->~Value();
    if (!counts(12346, 1, 1, 1, 1, 0, 1, 0)) return 30;
    reset();
    try { construct_discard(false, true); return 31; }
    catch (int &failure) { if (failure != 37) return 32; }
    if (!counts(12345, 1, 1, 1, 1, 1, 0, 0) || deleted_argument != (void *)arena) return 33;

    reset();
    Plain *copied = construct_copy(false);
    if (copied != (Plain *)arena || copied->value != 7 || copied->other != 11 ||
        allocated_bytes != sizeof(Plain) || !counts(123, 1, 1, 1, 0, 0, 0, 0)) return 34;
    copied->value = 23;
    copied->other = 19;
    if (plain_source.value != 7 || plain_source.other != 11) return 35;
    reset();
    try { construct_copy(true); return 36; }
    catch (int &failure) { if (failure != 29) return 37; }
    if (!counts(1235, 1, 1, 1, 0, 1, 0, 0) || deleted_result != (void *)arena || deleted_argument != (void *)arena) return 38;
    return 0;
}
