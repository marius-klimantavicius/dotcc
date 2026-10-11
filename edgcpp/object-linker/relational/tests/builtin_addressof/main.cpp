#include "program.h"

int main() {
    if (overloaded_address() != nullptr || overloads != 1) return 1;
    trace = 0;
    Item* first = record_address();
    if (trace != 1 || first->value != 17 || overloads != 1) return 2;
    int* field = field_address();
    if (trace != 11 || field != __builtin_addressof(first->value)) return 3;
    int* integer = scalar_address();
    if (trace != 112 || integer != __builtin_addressof(scalar)) return 4;
    first->value = 41;
    *integer = 43;
    if (*field != 41 || record.value != 41 || scalar != 43 || overloads != 1) return 5;
    if (first != __builtin_addressof(record)) return 6;
    int* element = array_element_address(1);
    *element = 47;
    if (values[1] != 47 || element != array_element_address(1)) return 7;
    int* persistent = static_address();
    if (*persistent != 37) return 8;
    *persistent = 53;
    if (static_address() != persistent || *static_address() != 53) return 9;
    int local = 59;
    int* local_address = __builtin_addressof(local);
    *local_address = 61;
    if (local != 61) return 10;
    Item local_record = {67};
    Item* local_record_address = __builtin_addressof(local_record);
    local_record_address->value = 71;
    if (local_record.value != 71 || overloads != 1) return 11;
    return 0;
}
