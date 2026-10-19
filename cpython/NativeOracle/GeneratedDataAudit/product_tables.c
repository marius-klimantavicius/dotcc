#include <stdint.h>
#include <stdio.h>

/* Actual symbols from the same-pin native CPython archive. No interpreter is
   initialized, and no emitter serializer supplies the expected object bytes. */
extern unsigned char _PyLong_DigitValue[256];
extern const unsigned int _Py_ctype_table[256];
extern const unsigned char _Py_ctype_tolower[256];
extern const unsigned char _Py_ctype_toupper[256];
extern const uint32_t _PyUnicode_ExtendedCase[1236];

static void show(const char *name, const void *data, size_t length) {
    const unsigned char *bytes = data;
    printf("%s %zu ", name, length);
    for (size_t i = 0; i < length; ++i) printf("%02x", bytes[i]);
    putchar('\n');
}
int main(void) {
    if (sizeof(unsigned char) != 1 || sizeof(unsigned int) != 4 || sizeof(uint32_t) != 4
        || *(const unsigned char *)&(const uint32_t){1} != 1) return 23;
    show("_PyLong_DigitValue", _PyLong_DigitValue, sizeof(_PyLong_DigitValue));
    show("_Py_ctype_table", _Py_ctype_table, sizeof(_Py_ctype_table));
    show("_Py_ctype_tolower", _Py_ctype_tolower, sizeof(_Py_ctype_tolower));
    show("_Py_ctype_toupper", _Py_ctype_toupper, sizeof(_Py_ctype_toupper));
    show("_PyUnicode_ExtendedCase", _PyUnicode_ExtendedCase, sizeof(_PyUnicode_ExtendedCase));
    return 0;
}
