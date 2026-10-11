#include "program.h"

int main() {
    char character = 'x';
    LongShape long_value = {1, 7, &character};
    ShortShape short_value = {0, 3, {11, 22, 33}};
    Packed value = {166, -1, 2, 63, false, 211};
    if (sizeof(Packed) != 5 || sizeof(ShortShape) != 4) return 1;
    update(&long_value, &short_value, &value);
    if (long_value.__is_long_ != 0 || long_value.__cap_ != 0x7123456789abcdefUL || long_value.__data_ != &character) return 2;
    if (short_value.__is_long_ != 1 || short_value.__size_ != 109 || short_value.__data_[0] != 11 || short_value.__data_[2] != 33) return 3;
    if (value.before != 166 || value.after != 211 || value.signed_value != -11 || value.low != 5 || value.crossing != 257 || !value.enabled) return 4;
    unsigned char* bytes = (unsigned char*)&value;
    if (bytes[0] != 166 || bytes[1] != 181 || bytes[2] != 1 || bytes[3] != 3 || bytes[4] != 211) return 5;
    value.low = 15;
    value.signed_value = 29;
    value.crossing = 1023;
    value.enabled = false;
    if (value.signed_value != -3 || value.low != 7 || value.crossing != 511 || value.enabled) return 6;
    if (bytes[1] != 253 || bytes[2] != 255 || bytes[3] != 1 || bytes[4] != 211) return 7;
    effects = 0;
    trace = 0;
    ordered_store(&value);
    if (effects != 1 || trace != 12 || value.signed_value != -9 || value.low != 7 || value.crossing != 511 || value.after != 211) return 8;
    value.enabled = true;
    value.low = 0;
    if (!value.enabled || value.crossing != 511 || value.signed_value != -9 || value.before != 166 || value.after != 211) return 9;
    return 0;
}
