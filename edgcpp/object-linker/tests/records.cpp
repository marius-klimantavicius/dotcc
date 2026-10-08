#include "record_fixture.h"

int record_checks(void) {
    struct RecordNode first;
    struct RecordNode second;
    first.next = &second;
    first.data.tag = 9;
    first.data.total = 10000000000LL;
    first.data.count = 4;
    first.data.values[0] = 6;
    first.data.values[1] = 7;
    first.data.values[2] = 8;
    first.trace = (int*)0;
    first.weight = 1.25;
    second.next = &first;
    second.data.count = 19;
    if (first.next->data.count != 19 || first.next->next != &first) return 1;
    record_adjust(&first);
    if (first.data.count != 5 || first.data.values[1] != 8 || first.weight != 1.75) return 2;
    if (record_sum(&first) != 10000000019LL) return 3;
    int* count = &first.data.count;
    *count = 12;
    if (first.data.count != 12) return 4;
    short* values = first.data.values;
    if (values + 2 != &first.data.values[2] || values[2] != 8) return 5;
    if (sizeof(first) != record_provider_size() || sizeof(first) != 56) return 6;
    if ((unsigned char*)&first.data - (unsigned char*)&first != 8) return 7;
    if ((unsigned char*)&first.data.total - (unsigned char*)&first != 16) return 8;
    if ((unsigned char*)&first.data.values - (unsigned char*)&first != 28) return 9;
    if ((unsigned char*)&first.trace - (unsigned char*)&first != 40) return 10;
    if (((unsigned long)&first & 7UL) || ((unsigned long)&first.data.total & 7UL)) return 11;
    if (RecordPair() != 37) return 12;
    if ((unsigned char*)(&first + 1) - (unsigned char*)&first != 56) return 13;
    return 0;
}

int record_initialization(void) {
    struct RecordPair pair = {11, 17};
    struct RecordPayload payload = {3, 99LL, 4, {5, 6}};
    if (pair.left != 11 || pair.right != 17) return 1;
    if (payload.tag != 3 || payload.total != 99 || payload.count != 4) return 2;
    if (payload.values[0] != 5 || payload.values[1] != 6 || payload.values[2] != 0) return 3;
#ifdef __cplusplus
    struct RecordPair zero{};
#else
    struct RecordPair zero = {0};
#endif
    if (zero.left || zero.right) return 4;
    struct RecordNode nested = {0, {2, 88LL, 3, {1, 2, 3}}, 0, 0.5};
    if (nested.next || nested.trace || nested.data.tag != 2 || nested.data.values[2] != 3 || nested.weight != 0.5) return 5;
    return 0;
}

void record_scope_cleanup(int* trace, int early) {
    struct RecordNode guard __attribute__((cleanup(record_cleanup)));
    guard.trace = trace;
    guard.data.count = 3;
    if (early) return;
    {
        struct RecordNode inner __attribute__((cleanup(record_cleanup)));
        inner.trace = trace;
        inner.data.count = 7;
    }
    *trace = *trace * 10 + 8;
}

int main(void) {
    int result = record_checks();
    if (result) return result;
    result = record_initialization();
    if (result) return 20 + result;
    int trace = 0;
    record_scope_cleanup(&trace, 1);
    if (trace != 3) return 30;
    trace = 0;
    record_scope_cleanup(&trace, 0);
    if (trace != 783) return 31;
    return 0;
}
