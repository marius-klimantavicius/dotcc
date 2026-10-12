#include "program.h"

long long arena[16], other_arena[16], trace;
int allocation_calls, argument_calls, construction_calls, deletion_calls, destruction_calls;
int allocation_mode;
size_type allocated_bytes, deleted_bytes;
void *current_storage, *deleted_storage;

void mark(int event) { trace = trace * 10 + event; }
void reset() {
    trace = 0;
    allocation_calls = argument_calls = construction_calls = deletion_calls = destruction_calls = 0;
    allocation_mode = 0;
    allocated_bytes = deleted_bytes = 0;
    current_storage = arena;
    deleted_storage = nullptr;
}
int input_value(bool fail) {
    ++argument_calls; mark(2);
    current_storage = other_arena;
    if (fail) throw 22;
    return 9;
}
void *Value::operator new(size_type bytes) {
    ++allocation_calls; allocated_bytes = bytes; mark(1);
    if (allocation_mode == 2) throw 21;
    return current_storage;
}
void Value::operator delete(void *storage, size_type bytes) noexcept {
    ++deletion_calls; deleted_storage = storage; deleted_bytes = bytes; mark(4);
}
Value::Value(int input, bool fail) : self(this), value(input) {
    ++construction_calls; mark(3);
    if (fail) throw 23;
}
Value::~Value() noexcept { ++destruction_calls; mark(5); }

void *Nullable::operator new(size_type bytes) noexcept {
    ++allocation_calls; allocated_bytes = bytes; mark(1);
    return allocation_mode == 1 ? nullptr : current_storage;
}
void Nullable::operator delete(void *storage) noexcept {
    ++deletion_calls; deleted_storage = storage; mark(4);
}
Nullable::Nullable(int input) : self(this), value(input) { ++construction_calls; mark(3); }

Value *construct_value(bool fail_argument, bool fail_constructor) {
    return new Value(input_value(fail_argument), fail_constructor);
}
Nullable *construct_nullable(bool fail_argument) {
    return new Nullable(input_value(fail_argument));
}
