#ifndef RECORD_FIXTURE_H
#define RECORD_FIXTURE_H
struct RecordNode;
struct RecordPair {
    int left;
    int right;
};
struct RecordPayload {
    unsigned char tag;
    long long total;
    int count;
    short values[3];
};
struct RecordNode {
    struct RecordNode* next;
    struct RecordPayload data;
    int* trace;
    double weight;
};
#ifdef __cplusplus
extern "C" {
#endif
void record_adjust(struct RecordNode* node);
long long record_sum(const struct RecordNode* node);
void record_cleanup(struct RecordNode* node);
int RecordPair(void);
unsigned long record_provider_size(void);
#ifdef __cplusplus
}
#endif
#endif
