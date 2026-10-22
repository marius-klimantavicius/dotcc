#include <stdio.h>
#include <stdbool.h>

enum WalLevel { MINIMAL = 0, REPLICA = 1, LOGICAL = 2 };
typedef struct QueuePosition { long page; int offset; } QueuePosition;
typedef struct NameData { char data[64]; } NameData;
typedef struct Record { NameData name; QueuePosition position; } Record;
typedef struct ThreeBytes { unsigned char a, b, c; } ThreeBytes;

static volatile bool ready = true;
static volatile enum WalLevel level = LOGICAL;
static volatile Record record = { { "abc" }, { 17, 3 } };
static volatile ThreeBytes small = { 4, 5, 6 };
static int addresses;
static volatile QueuePosition *position(void) { addresses++; return &record.position; }

static long advance(volatile QueuePosition *p) {
    long page = ((*p)).page;
    int offset = ((*p)).offset;
    ((*p)).page = page + 2;
    ((*p)).offset = offset + 7;
    return ((*p)).page + ((*p)).offset;
}

int main(void) {
    if (!ready || level != LOGICAL || record.position.page != 17) return 1;
    if (record.name.data[0] != 'a' || record.name.data[2] != 'c') return 2;
    if (advance(position()) != 29 || addresses != 1) return 3;
    Record copy = record;
    record.position.offset = 30;
    if (copy.position.offset != 10 || copy.position.page != 19) return 4;
    if (copy.name.data[1] != 'b' || copy.name.data[63] != 0) return 5;
    ThreeBytes three = small;
    small = (ThreeBytes){ 7, 8, 9 };
    if (three.a != 4 || three.b != 5 || three.c != 6 || small.b != 8) return 6;
    Record replacement = { { "xyz" }, { 80, 90 } };
    record = replacement;
    replacement.position.page = -1;
    if (record.position.page != 80 || record.name.data[2] != 'z') return 7;
    ready = false;
    level = REPLICA;
    if (ready || level != REPLICA) return 8;
    printf("loads %ld %d %d\n", copy.position.page, copy.position.offset, addresses);
    return 0;
}
