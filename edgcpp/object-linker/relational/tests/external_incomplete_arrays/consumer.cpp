#include "program.h"
const word* values_address() { return values; }
const unsigned char* bytes_address() { return bytes; }
const word (*whole_values())[] { return &values; }
word read_value(int index) { return values[index]; }
int run_arrays() {
    if (read_value(0) != 7 || read_value(1) != 11 || read_value(2) != 19 || read_value(3) != 23) return 1;
    if (bytes[0] != 2 || bytes[1] != 3 || bytes[2] != 5) return 2;
    if (reinterpret_cast<const void*>(whole_values()) != reinterpret_cast<const void*>(values_address())) return 3;
    if (matrix[0][0] != 13 || matrix[0][1] != 15 || matrix[1][0] != 17 || matrix[1][1] != 19) return 4;
    return 0;
}
