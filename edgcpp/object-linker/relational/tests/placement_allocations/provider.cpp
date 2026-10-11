#include "program.h"

long long arena[8], other_arena[8], trace;
int placement_calls, allocation_calls, constructor_arguments, constructor_calls, deletion_calls, destructions, alive;
int tag_value, allocated_tag, deleted_tag;
size_type allocated_bytes;
void *current_pointer, *deleted_result, *deleted_argument;
bool return_null, allocator_failure;
Plain plain_source = {7, 11};

void mark(int event) { trace = trace * 10 + event; }
void reset() {
    trace = 0;
    placement_calls = allocation_calls = constructor_arguments = constructor_calls = deletion_calls = destructions = alive = 0;
    tag_value = 7;
    allocated_tag = deleted_tag = 0;
    allocated_bytes = 0;
    current_pointer = arena;
    deleted_result = deleted_argument = nullptr;
    return_null = allocator_failure = false;
    plain_source.value = 7;
    plain_source.other = 11;
}
void *pointer_argument(bool fail) {
    ++placement_calls; mark(1);
    if (fail) throw 23;
    return current_pointer;
}
int tag_argument() { ++placement_calls; mark(1); return tag_value; }
long long wide_tag_argument() { ++placement_calls; mark(1); return tag_value; }
int input_value(bool fail) {
    ++constructor_arguments; mark(3);
    current_pointer = other_arena;
    tag_value = 99;
    if (fail) throw 29;
    return 9;
}
void *operator new(size_type bytes, void *address) noexcept {
    ++allocation_calls; allocated_bytes = bytes; mark(2);
    return address;
}
void operator delete(void *result, void *address) noexcept {
    ++deletion_calls; mark(5); deleted_result = result; deleted_argument = address;
}
void *operator new(size_type bytes, int tag) noexcept {
    ++allocation_calls; allocated_bytes = bytes; allocated_tag = tag; mark(2);
    if (return_null) return nullptr;
    return arena;
}
void operator delete(void *result, int tag) noexcept {
    ++deletion_calls; mark(5); deleted_result = result; deleted_tag = tag;
}
void *operator new(size_type bytes, long long tag) {
    ++allocation_calls; allocated_bytes = bytes; allocated_tag = int(tag); mark(2);
    if (allocator_failure) throw 41;
    return arena;
}
void operator delete(void *result, long long tag) noexcept {
    ++deletion_calls; mark(5); deleted_result = result; deleted_tag = int(tag);
}

Value::Value(int input, bool fail) : self(this), value(input) {
    ++constructor_calls; mark(4);
    if (fail) throw 37;
    ++alive;
}
Value::~Value() noexcept { ++destructions; --alive; mark(6); }
bool Value::stable() const { return self == this; }

Value *construct_pointer(bool fail_placement, bool fail_argument, bool fail_constructor) {
    return new (pointer_argument(fail_placement)) Value(input_value(fail_argument), fail_constructor);
}
Value *construct_tag(bool fail_argument, bool fail_constructor) {
    return new (tag_argument()) Value(input_value(fail_argument), fail_constructor);
}
Value *construct_throwing(bool fail_argument, bool fail_constructor) {
    return new (wide_tag_argument()) Value(input_value(fail_argument), fail_constructor);
}
void construct_discard(bool fail_argument, bool fail_constructor) {
    (void)::new (pointer_argument(false)) Value(input_value(fail_argument), fail_constructor);
}
const Plain &plain_argument(bool fail) {
    ++constructor_arguments; mark(3);
    current_pointer = other_arena;
    if (fail) throw 29;
    return plain_source;
}
Plain *construct_copy(bool fail_argument) {
    return new (pointer_argument(false)) Plain(plain_argument(fail_argument));
}
