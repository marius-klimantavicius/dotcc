#include "program.h"

long long arena[32];
long long value_arena[8];
long long other_arena[8];
int chosen_count = 0;
int count_calls = 0;
int allocation_calls = 0;
size_type allocated_bytes = 0;
bool return_null = false;
int failure_calls = 0;
int pointer_calls = 0;
int value_destructions = 0;
int member_destructions = 0;
int deallocation_calls = 0;
void* deleted_pointer = 0;
size_type deleted_bytes = 0;
long long trace = 0;
Value* current_value = 0;

void reset() {
    chosen_count = 3;
    count_calls = 0;
    allocation_calls = 0;
    allocated_bytes = 0;
    return_null = false;
    failure_calls = 0;
    pointer_calls = 0;
    value_destructions = 0;
    member_destructions = 0;
    deallocation_calls = 0;
    deleted_pointer = 0;
    deleted_bytes = 0;
    trace = 0;
    unsigned char* bytes = (unsigned char*)arena;
    for (int index = 0; index < (int)sizeof(arena); ++index) bytes[index] = 165;
}

void* operator new[](size_type bytes) noexcept {
    ++allocation_calls;
    allocated_bytes = bytes;
    chosen_count = 99;
    if (return_null) return 0;
    return arena;
}
void* operator new(size_type, void* pointer) noexcept { return pointer; }
void operator delete(void*, void*) noexcept { }
extern "C" [[noreturn]] void __cxa_throw_bad_array_new_length() { ++failure_calls; throw 79; }
int next_count() { ++count_calls; return chosen_count; }
unsigned long long* allocate_zero_words() { return new unsigned long long[next_count()](); }

Member::Member(int input) : value(input) { }
Member::~Member() noexcept { ++member_destructions; trace = trace * 10 + 3; }
Value::Value(int input) : member(input) { }
Value::~Value() noexcept {
    ++value_destructions;
    trace = trace * 10 + 2;
    current_value = (Value*)other_arena;
}
void Value::operator delete(void* pointer, size_type bytes) noexcept {
    ++deallocation_calls;
    deleted_pointer = pointer;
    deleted_bytes = bytes;
    trace = trace * 10 + 4;
}
Value* construct_value(int input) {
    Value* result = ::new ((void*)value_arena) Value(input);
    current_value = result;
    return result;
}
Value* selected_value(bool null_pointer) {
    ++pointer_calls;
    trace = trace * 10 + 1;
    return null_pointer ? (Value*)0 : current_value;
}
void release_value(bool null_pointer) { delete selected_value(null_pointer); }
