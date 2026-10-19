#include "types.h"
int sum_slots(struct slots *value) { return *value->values[2]+value->rows[1][2]+value->scalar; }
