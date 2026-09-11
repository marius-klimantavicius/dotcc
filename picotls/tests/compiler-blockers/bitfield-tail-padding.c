/* A scalar may reuse the unused bytes of the previous bitfield storage unit
   in the native LP64 ABI, as in ptls_aead_algorithm_t.align_bits. */
#include <stddef.h>
#include <stdio.h>
struct packed_tail {
    unsigned long prefix;
    unsigned non_temporal : 1;
    unsigned char align_bits;
    size_t context_size;
};
int main(void)
{
    struct packed_tail value = {0};
    value.non_temporal = 1;
    value.align_bits = 7;
    printf("size=%zu offset=%zu address=%ld bit=%u byte=%u\n",
           sizeof(value), offsetof(struct packed_tail, align_bits),
           (unsigned char *)&value.align_bits - (unsigned char *)&value,
           value.non_temporal, (unsigned int)value.align_bits);
    return 0;
}
