/* Runtime coverage for declaration shapes encountered in QuickJS. */
#include <stdio.h>
#include <stddef.h>
#include <stdint.h>
#include <stdlib.h>

typedef struct Header {
    unsigned short index;
    unsigned char size, mark;
    int refs;
    __attribute__((aligned(16))) unsigned char data[];
} Header;
struct Value { long payload, tag; };
struct Data { int index; struct Value iter, next, values[]; };
static const int table[3];
static const int *get_table(void) { return table; }
static const int table[3] = {3, 5, 7};
static const int table[3];

static __attribute__((noinline)) __attribute__((warn_unused_result)) int reference_count(Header *header)
{
    return header->refs;
}
static inline __attribute__((always_inline)) Header *identity(Header *header)
{
    return header;
}
static int sum(double fields[static 9])
{
    int result = 0;
    for (int i = 0; i < 9; i++) result += fields[i];
    return result;
}
int main(void)
{
    if (get_table() != table || get_table()[0] != 3 || get_table()[1] != 5 || get_table()[2] != 7) return 6;
    Header *header = malloc(sizeof(Header) + 8);
    struct Data *data = malloc(sizeof(struct Data) + 3 * sizeof(struct Value));
    if (!header || !data) return 1;
    header->index = 123;
    header->size = 8;
    header->mark = 7;
    header->refs = 17;
    int bytes = 0;
    for (int i = 0; i < 8; i++) header->data[i] = i + 1;
    for (int i = 0; i < 8; i++) bytes += identity(header)->data[i];
    if ((unsigned char *)header->data - (unsigned char *)header != 16) return 2;
    if (header->index != 123 || header->size != 8 || header->mark != 7 || reference_count(header) != 17) return 3;
    data->index = 42;
    data->iter.payload = 51;
    data->next.tag = 61;
    long payload = 0, tags = 0;
    for (int i = 0; i < 3; i++) {
        data->values[i].payload = i + 10;
        data->values[i].tag = -(i + 1);
    }
    for (int i = 0; i < 3; i++) { payload += data->values[i].payload; tags += data->values[i].tag; }
    if ((unsigned char *)data->values - (unsigned char *)data != 40) return 4;
    if (data->index != 42 || data->iter.payload != 51 || data->next.tag != 61) return 5;
    double fields[9] = {1,2,3,4,5,6,7,8,9};
    printf("header %zu %zu %zu bytes %d refs %d\n", sizeof(Header), _Alignof(Header), offsetof(Header, data), bytes, reference_count(header));
    printf("comma-tail %zu %zu values %ld %ld minimum-sum %d\n", sizeof(struct Data), offsetof(struct Data, values), payload, tags, sum(fields));
    free(data);
    free(header);
    return 0;
}
