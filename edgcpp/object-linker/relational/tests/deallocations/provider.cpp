#include "program.h"
unsigned char arena[256];
size_type allocation_bytes = 0;
int allocation_calls = 0;
int count_calls = 0;
int failure_calls = 0;
extern "C" [[noreturn]] void __cxa_throw_bad_array_new_length();
void *operator new[](size_type bytes) {
    if (bytes > sizeof(arena)) __cxa_throw_bad_array_new_length();
    ++allocation_calls;
    allocation_bytes = bytes;
    if (bytes == 13) throw 47;
    return arena;
}
extern "C" [[noreturn]] void __cxa_throw_bad_array_new_length() {
    ++failure_calls;
    throw 91;
}
int next_count(int value) { ++count_calls; return value; }
char *allocate_bytes(size_type count) { return new char[count + 1]; }
int deallocation_calls = 0;
int pointer_calls = 0;
void operator delete[](void *pointer) noexcept {
    if (!pointer) return;
    ++deallocation_calls;
}
char *next_pointer(char *value) { ++pointer_calls; return value; }
void release_bytes(char *value) { delete[] next_pointer(value); }
