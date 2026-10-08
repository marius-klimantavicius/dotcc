unsigned char* pointer_byte(unsigned char* value) { return value; }
#include "pointer_fixture.h"

int local_pointer_checks(void)
{
    int first = 7;
    int second = 11;
    IntPointer ptr = &first;
    int** slot = &ptr;
    ConstIntPointer const_ptr = ptr;
    int* const fixed_ptr = &first;
    if (*fixed_ptr != 7) return 18;
    if (pointer_read(const_ptr) != 7) return 1;
    pointer_write(ptr, 13);
    if (first != 13 || *ptr != 13) return 2;
    if (pointer_post_increment(ptr) != 13 || first != 14) return 3;
    if (++*ptr != 15 || --*ptr != 14 || (*ptr)-- != 14 || first != 13) return 4;
    if (&*ptr != ptr || **slot != first) return 5;
    pointer_replace(slot, &second);
    if (ptr != &second || *ptr != 11) return 6;
    if (pointer_choose(&first, &second, 1) != &first) return 7;
    if (pointer_choose(&first, &second, 0) != &second) return 8;

    void* opaque = ptr;
    int* restored = (int*)opaque;
    unsigned long long numeric_address = (unsigned long long)restored;
    if ((int*)numeric_address != ptr || *restored != 11) return 9;
    int* missing = (int*)0;
#ifdef __cplusplus
    int* null_keyword = nullptr;
    if (null_keyword != nullptr || null_keyword != missing) return 30;
#endif
    if (missing || !ptr || missing != (int*)0) return 10;
    if (!(ptr && !missing) || (missing && *missing)) return 11;
    int selected = missing ? *missing : *ptr;
    if (selected != 11 || (missing ? missing : ptr) != ptr) return 12;
#ifndef __cplusplus
    if (&*missing != missing) return 13;
#endif

    unsigned long long wide = 18446744073709551615ULL;
    if (*pointer_wide(&wide) != 18446744073709551615ULL) return 14;
    unsigned char byte = 255;
    unsigned char* byte_ptr = pointer_byte(&byte);
    ++*byte_ptr;
    if (byte != 0 || (*byte_ptr)-- != 0 || byte != 255) return 15;
#ifdef __cplusplus
    bool flag = true;
    bool* flag_ptr = &flag;
#else
    _Bool flag = 1;
    _Bool* flag_ptr = &flag;
#endif
    if (!*flag_ptr) return 19;
    *flag_ptr = 0;
    if (flag || *flag_ptr) return 20;
    return 0;
}

int pointer_arithmetic_checks(void)
{
    // A scalar object is an array of length one for these defined operations.
    int value = 23;
    int* begin = &value;
    int* end = begin + 1;
    if (end - begin != 1 || begin - end != -1) return 21;
    if (!(begin < end) || end <= begin || !(end > begin) || begin >= end) return 22;
    if (*(end - 1) != 23 || (1 + begin) != end || begin[0] != 23) return 23;
    int* cursor = begin;
    if (cursor++ != begin || cursor != end) return 24;
    if (--cursor != begin || ++cursor != end) return 25;
    if (cursor-- != end || cursor != begin) return 26;
    *cursor = value + 4;
    if (value != 27) return 27;
    unsigned long long offset = 1;
    if (begin + offset != end || end - offset != begin) return 28;
    int** slot = &cursor;
    int** next_slot = slot + 1;
    if (next_slot - slot != 1 || *(next_slot - 1) != cursor) return 29;
    begin[0] = 31;
    if (++begin[0] != 32 || &begin[0] != begin) return 31;
    return 0;
}

int main(void)
{
    int result = local_pointer_checks();
    if (result) return result;
    return pointer_arithmetic_checks();
}
