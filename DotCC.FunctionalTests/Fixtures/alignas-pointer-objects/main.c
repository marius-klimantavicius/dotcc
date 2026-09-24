#include <stdio.h>
#include <stddef.h>
#include <stdint.h>
struct Queue { char lead; _Alignas(64) void **buffer; };
struct Big { _Alignas(64) int value; };
struct Holder { char lead; _Alignas(8) struct Big *pointer; };
struct Typed { _Alignas(void *) void **pointer; };
struct Combined { _Alignas(1) _Alignas(64) int value; };
struct Zero { _Alignas(0) void **pointer; };
_Alignas(64) void **global_pointer;
int main(void) {
    _Alignas(64) void **local_pointer = 0;
    static _Alignas(64) void **cached_pointer;
    printf("%zu %zu %zu %zu %zu %zu %zu %zu\n",
        sizeof(struct Queue), _Alignof(struct Queue), offsetof(struct Queue, buffer),
        sizeof(struct Holder), _Alignof(struct Holder), offsetof(struct Holder, pointer),
        _Alignof(struct Typed), _Alignof(struct Zero));
    printf("%zu %zu %d %d %d %d\n", sizeof(struct Combined), _Alignof(struct Combined),
        (uintptr_t)&global_pointer % 64 == 0, (uintptr_t)&local_pointer % 64 == 0,
        (uintptr_t)&cached_pointer % 64 == 0,
        local_pointer == cached_pointer && cached_pointer == global_pointer);
    return 0;
}
