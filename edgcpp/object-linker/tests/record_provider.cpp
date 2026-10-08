#include "record_fixture.h"
void record_adjust(struct RecordNode* node) {
    ++node->data.count;
    ++node->data.values[1];
    node->weight = node->weight + 0.5;
}
long long record_sum(const struct RecordNode* node) {
    return node->data.total + node->data.count + node->data.values[0] + node->data.values[1];
}
void record_cleanup(struct RecordNode* node) {
    *node->trace = *node->trace * 10 + node->data.count;
}
int RecordPair(void) { return 37; }
unsigned long record_provider_size(void) { return sizeof(struct RecordNode); }
