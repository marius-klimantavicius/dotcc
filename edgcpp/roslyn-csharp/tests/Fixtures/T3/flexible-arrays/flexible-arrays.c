struct Tail { int count; int tag; int values[]; };
struct PaddedTail { unsigned long marker; int count; int values[]; };
void *malloc(unsigned long size);
void free(void *value);
static long observations[12];
static int receiver_count;
static struct Tail *selected_tail(struct Tail *value) { ++receiver_count; return value; }
int *tail_data(struct Tail *value) { return value->values; }
int *padded_data(struct PaddedTail *value) { return value->values; }
void write_tail(struct Tail *value) {
    value->count = 3;
    value->tag = 17;
    selected_tail(value)->values[0] = 11;
    value->values[1] = 23;
    value->values[2] = value->values[0] + value->values[1];
}
int fixture(void) {
    void *backing = malloc(64);
    if (!backing) return 0;
    struct Tail *value = (struct Tail *)backing;
    receiver_count = 0;
    write_tail(value);
    observations[0] = value->count;
    observations[1] = value->tag;
    observations[2] = tail_data(value) == value->values;
    observations[3] = (unsigned char *)tail_data(value) - (unsigned char *)value;
    observations[4] = value->values[0];
    observations[5] = value->values[1];
    observations[6] = value->values[2];
    observations[7] = receiver_count;
    struct PaddedTail *padded = (struct PaddedTail *)backing;
    padded->marker = 41;
    padded->count = 2;
    padded->values[0] = 43;
    padded->values[1] = 47;
    observations[8] = (unsigned char *)padded_data(padded) - (unsigned char *)padded;
    observations[9] = padded->values[0] + padded->values[1];
    observations[10] = sizeof(struct Tail);
    observations[11] = sizeof(struct PaddedTail);
    free(backing);
    return 12;
}
long flexible_observe(int index) { return observations[index]; }
