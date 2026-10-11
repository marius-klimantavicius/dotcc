#include "program.h"

int main() {
    reset();
    unsigned long long* words = allocate_zero_words();
    if ((void*)words != (void*)arena || count_calls != 1 || allocation_calls != 1 ||
        allocated_bytes != 3 * sizeof(unsigned long long) || chosen_count != 99) return 1;
    if (words[0] != 0 || words[1] != 0 || words[2] != 0 || ((unsigned char*)arena)[24] != 165) return 2;
    reset();
    chosen_count = 0;
    if ((void*)allocate_zero_words() != (void*)arena || count_calls != 1 || allocation_calls != 1 ||
        allocated_bytes != 0 || ((unsigned char*)arena)[0] != 165) return 3;
    reset();
    return_null = true;
    if (allocate_zero_words() != 0 || count_calls != 1 || allocation_calls != 1 ||
        allocated_bytes != 24 || ((unsigned char*)arena)[0] != 165) return 4;
    reset();
    Value* value = construct_value(17);
    if ((void*)value != (void*)value_arena || value->member.value != 17) return 5;
    release_value(false);
    if (pointer_calls != 1 || value_destructions != 1 || member_destructions != 1 ||
        deallocation_calls != 1 || deleted_pointer != (void*)value_arena ||
        deleted_bytes != sizeof(Value) || trace != 1234) return 6;
    reset();
    release_value(true);
    if (pointer_calls != 1 || value_destructions != 0 || member_destructions != 0 ||
        deallocation_calls != 0 || trace != 1) return 7;
    return 0;
}
