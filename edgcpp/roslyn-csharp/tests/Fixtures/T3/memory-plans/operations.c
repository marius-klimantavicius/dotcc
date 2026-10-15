/* Genuine C17 bodies: immediate pointer stride, exact shifts, volatile
   fetch/store occurrences and selected variadic argument prefixes. */
typedef __builtin_va_list fixture_va_list;
struct Item { int first; int second; int third; };
struct VolatileHeader { unsigned short first; unsigned int second; };
static long long observations[24];
static int events[8];
static int event_count;
static int selections;
static unsigned char bytes[12];
static unsigned char *cursor;
static volatile unsigned short halfwords[3];
static volatile unsigned int words[3];
static volatile struct VolatileHeader header;

static int mark(int event, int value) {
    events[event_count++] = event;
    return value;
}
static unsigned char **selected_cursor(void) { ++selections; return &cursor; }
static int sum(int count, ...) {
    fixture_va_list list;
    __builtin_va_start(list, count);
    int result = 0;
    for (int index = 0; index < count; ++index) result += __builtin_va_arg(list, int);
    __builtin_va_end(list);
    return result;
}

int fixture(void) {
    event_count = 0; selections = 0;
    cursor = bytes;
    *selected_cursor() += 5;
    observations[0] = cursor - bytes;
    *selected_cursor() -= 2;
    observations[1] = cursor - bytes;
    observations[2] = selections;
    struct Item items[3];
    struct Item *links[3];
    links[0] = items; links[1] = items + 1; links[2] = items + 2;
    struct Item **link = links;
    link += 2;
    observations[3] = link - links;
    observations[4] = *link - items;
    link -= 1;
    observations[5] = link - links;
    struct Item *item = items;
    item += 2;
    observations[6] = item - items;
    item -= 1;
    observations[7] = item - items;
    unsigned long long bits = 3;
    bits <<= 33;
    observations[8] = (long long)bits;
    bits >>= 32;
    observations[9] = (long long)bits;
    bits = 0x8000000000000000ULL;
    observations[10] = (long long)(bits >> 63);
    observations[11] = (long long)(1ULL << 63) == (-9223372036854775807LL - 1);
    halfwords[1] = 65530;
    words[2] = 0x12345678U;
    header.first = 17;
    header.second = 23;
    observations[12] = halfwords[1];
    observations[13] = words[2];
    observations[14] = header.first;
    observations[15] = header.second;
    volatile unsigned int *address_only = &words[2];
    observations[16] = address_only == words + 2;
    (void)words[2];
    observations[17] = (mark(1, 0), sum(2, (mark(2, 0), 11), 13));
    int selected = (int)words[2];
    observations[18] = selected == 0 ? sum(1, (mark(9, 0), 99)) : sum(1, (mark(3, 0), 17));
    observations[19] = selected != 0 ? sum(1, (mark(4, 0), 19)) : sum(1, (mark(9, 0), 99));
    observations[20] = event_count;
    observations[21] = events[0] * 1000 + events[1] * 100 + events[2] * 10 + events[3];
    observations[22] = sizeof(struct Item);
    observations[23] = sizeof(struct Item *);
    return 24;
}
int memory_observation_count(void) { return 24; }
long long memory_observe(int index) { return observations[index]; }
int memory_event_count(void) { return event_count; }
int memory_event(int index) { return events[index]; }
