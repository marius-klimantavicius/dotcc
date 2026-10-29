#include <stdint.h>
#include <stddef.h>
#include <stdio.h>

typedef __attribute__((aligned(1))) uint64_t Under;
typedef uint64_t Post __attribute__((aligned(2)));
typedef __attribute__((aligned(32))) uint64_t Over;
typedef Under Chain;
struct Record { char lead; Under value; char tail; };
static Over global_value;

_Static_assert(sizeof(Under) == 8, "scalar size unchanged");
_Static_assert(_Alignof(Under) == 1, "weakened typedef alignment");
_Static_assert(_Alignof(Post) == 2, "postfix typedef alignment");
_Static_assert(_Alignof(Over) == 32, "strengthened typedef alignment");
_Static_assert(_Alignof(Chain) == 1, "chained typedef alignment");
_Static_assert(sizeof(struct Record) == 10, "unaligned member layout");
_Static_assert(offsetof(struct Record, value) == 1, "member offset");
_Static_assert(_Generic((Under)0, uint64_t: 1, default: 0), "compatible scalar type");

int main(void) {
    unsigned char bytes[18] = {0};
    Under *unaligned = (Under *)(bytes + 1);
    *unaligned = 0x0102030405060708ULL;
    Over local_value = 42;
    global_value = local_value + 1;
    struct Record record = {0};
    record.value = *unaligned;
    record.tail = 7;
    printf("layout %d %d %d %d %d %d %d\n", (int)sizeof(Under),
        (int)_Alignof(Under), (int)_Alignof(Post), (int)_Alignof(Over),
        (int)_Alignof(Chain), (int)sizeof(record), (int)offsetof(struct Record, value));
    printf("storage %d %d values %d %d %d %d\n",
        (int)((uintptr_t)&local_value & 31), (int)((uintptr_t)&global_value & 31),
        (int)bytes[1], (int)bytes[8], (int)record.tail, (int)global_value);
    return 0;
}
