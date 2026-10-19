#ifndef TYPES_H
#define TYPES_H
struct slots { int *(values[3]); int ((rows[2])[3]); int (scalar); };
int sum_slots(struct slots *value);
#endif
