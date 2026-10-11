#include "program.h"

Item record = {17};
int scalar = 19;
int values[3] = {23, 29, 31};
int trace;
int overloads;

Item* Item::operator&() {
    ++overloads;
    return nullptr;
}

Item* overloaded_address() {
    return &record;
}

Item& select_record() {
    trace = trace * 10 + 1;
    return record;
}

int& select_scalar() {
    trace = trace * 10 + 2;
    return scalar;
}

Item* record_address() {
    return __builtin_addressof(select_record());
}

int* field_address() {
    return __builtin_addressof(select_record().value);
}

int* scalar_address() {
    return __builtin_addressof(select_scalar());
}

int* array_element_address(int index) {
    return __builtin_addressof(values[index]);
}

int* static_address() {
    static int stored = 37;
    return __builtin_addressof(stored);
}
