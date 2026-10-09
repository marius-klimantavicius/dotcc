#ifndef RECORD_TRANSPORT_SHARED_H
#define RECORD_TRANSPORT_SHARED_H
struct CrossValue {
    int value;
    CrossValue &operator=(const CrossValue &);
};
extern "C" CrossValue cross_make(int value);
extern "C" int cross_read();
extern "C" int cross_assign(CrossValue *target, const CrossValue *source);
extern "C" int cross_assignment_count();
extern "C" int cross_nonnull(CrossValue *value);
#endif
