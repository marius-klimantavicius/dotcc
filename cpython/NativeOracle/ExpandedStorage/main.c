/* Final-tool D10 control. The original A25 C fixture remains unchanged. */
/* Use the selected target's actual NAN expression in both C compilers, rather
   than the host math header's unrelated builtin NaN constant. */
#include <math.h>
#include <stddef.h>
#undef NAN
#define NAN (0.0f / 0.0f)
#define main a25_original_main
#include "../../../DotCC.FunctionalTests/Fixtures/d10-typed-storage-a25/main.c"
#undef main

enum E8 : signed char { e8_min = -128, e8_max = 127 };
enum U8 : unsigned char { u8_min = 0, u8_max = 255 };
enum E16 : short { e16_min = -32768, e16_max = 32767 };
enum U16 : unsigned short { u16_min = 0, u16_max = 65535 };
enum E32 : int { e32_min = (-2147483647-1), e32_max = 2147483647 };
enum U32 : unsigned int { u32_min = 0, u32_max = 4294967295U };
enum E64 : long { e64_min = (-9223372036854775807L-1), e64_max = 9223372036854775807L };
enum U64 : unsigned long { u64_zero = 0 };
enum E8 enum8[] = {e8_min, e8_max};
enum U8 uenum8[] = {u8_min, u8_max};
enum E16 enum16[] = {e16_min, e16_max};
enum U16 uenum16[] = {u16_min, u16_max};
enum E32 enum32[] = {e32_min, e32_max};
enum U32 uenum32[] = {u32_min, u32_max};
enum E64 enum64[] = {e64_min, e64_max};
enum U64 uenum64[] = {u64_zero, (enum U64)18446744073709551615UL};
char16_t code_units[] = {0, 65, 0xd800, 0xffff};
wchar_t wide_units[] = {0, 65, 0xd800, 0xffff};
char32_t scalar_units[] = {0, 0x10ffff, 0xffffffffU};
size_t native_sizes[] = {0, (size_t)-1, 0x8000000000000000UL};
ptrdiff_t native_differences[] = {(-9223372036854775807L-1), 0, 9223372036854775807L};
unsigned char terminators[24] = "A\0B\xc3\xa6\xf0\x9f\xa6\x8a\0Z";
#define PAD64 "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
#define PAD256 PAD64 PAD64 PAD64 PAD64
#define PAD1024 PAD256 PAD256 PAD256 PAD256
unsigned char utf8_chunks[] = PAD1024 PAD1024 PAD1024 PAD256 PAD256 PAD256
    PAD64 PAD64 PAD64 "AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA"
    "\xf0\x9f\xa6\x8a\0tail";
unsigned char zero_extents[12293] = {[0] = 17, [4095] = 19, [4096] = 23, [12292] = 29};
struct ArrayRow { unsigned short data[8201]; unsigned long tail; };
struct ArrayRow array_rows[2] = {
    {.data = {[0] = 37, [4095] = 41, [4096] = 43, [8200] = 47}, .tail = 53},
    {.data = {[0] = 59, [8200] = 61}, .tail = 67}
};

int expanded_check(void) {
    return a25_check() && sizeof(enum8[0]) == 1 && sizeof(enum16[0]) == 2
        && sizeof(enum32[0]) == 4 && sizeof(enum64[0]) == 8
        && enum8[0] == -128 && uenum8[1] == 255
        && enum16[0] == -32768 && uenum16[1] == 65535
        && enum32[0] == (-2147483647-1) && uenum32[1] == 4294967295U
        && enum64[0] == (-9223372036854775807L-1) && (unsigned long)uenum64[1] == 18446744073709551615UL
        && code_units[2] == 0xd800 && code_units[3] == 0xffff
        && sizeof(wide_units[0]) == 2 && wide_units[2] == 0xd800 && wide_units[3] == 0xffff
        && scalar_units[1] == 0x10ffff && scalar_units[2] == 0xffffffffU
        && sizeof(native_sizes[0]) == 8 && native_sizes[1] == 18446744073709551615UL
        && native_sizes[2] == 0x8000000000000000UL
        && sizeof(native_differences[0]) == 8 && native_differences[0] == (-9223372036854775807L-1)
        && native_differences[2] == 9223372036854775807L
        && terminators[0] == 'A' && terminators[1] == 0 && terminators[2] == 'B'
        && terminators[8] == 0x8a && terminators[9] == 0 && terminators[10] == 'Z'
        && terminators[11] == 0 && terminators[23] == 0
        && sizeof(utf8_chunks) == 4105 && utf8_chunks[4094] == 'A'
        && utf8_chunks[4095] == 0xf0 && utf8_chunks[4098] == 0x8a
        && utf8_chunks[4099] == 0 && utf8_chunks[4100] == 't' && utf8_chunks[4104] == 0
        && zero_extents[4095] == 19 && zero_extents[4096] == 23
        && zero_extents[4097] == 0 && zero_extents[12291] == 0 && zero_extents[12292] == 29
        && array_rows[0].data[4095] == 41 && array_rows[0].data[4096] == 43
        && array_rows[0].data[8199] == 0 && array_rows[0].data[8200] == 47 && array_rows[0].tail == 53
        && array_rows[1].data[8199] == 0 && array_rows[1].data[8200] == 61 && array_rows[1].tail == 67;
}

int expanded_run(void) {
    int result = a25_original_main();
    show(enum8, sizeof(enum8)); show(uenum8, sizeof(uenum8));
    show(enum16, sizeof(enum16)); show(uenum16, sizeof(uenum16));
    show(enum32, sizeof(enum32)); show(uenum32, sizeof(uenum32));
    show(enum64, sizeof(enum64)); show(uenum64, sizeof(uenum64));
    show(code_units, sizeof(code_units)); show(terminators, sizeof(terminators));
    show(wide_units, sizeof(wide_units)); show(scalar_units, sizeof(scalar_units));
    show(native_sizes, sizeof(native_sizes)); show(native_differences, sizeof(native_differences));
    show(utf8_chunks, sizeof(utf8_chunks));
    show(chunks, sizeof(chunks)); show(containing.data, sizeof(containing.data));
    show(zero_extents, sizeof(zero_extents));
    show(array_rows[0].data, sizeof(array_rows[0].data));
    show(array_rows[1].data, sizeof(array_rows[1].data));
    printf("offsets %lu %lu %lu %lu %lu\n", (unsigned long)sizeof(struct Row),
        (unsigned long)((char*)&rows[0].next-(char*)&rows[0]),
        (unsigned long)sizeof(struct ArrayRow),
        (unsigned long)((char*)&array_rows[0].tail-(char*)&array_rows[0]),
        (unsigned long)((char*)&array_rows[1]-(char*)&array_rows[0]));
    printf("expanded %d\n", expanded_check());
    return result || !expanded_check();
}

int main(void) { return expanded_run(); }
