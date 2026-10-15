/* Actual pass3 representation activations, independent of the first fixture. */
struct Wide { volatile int interrupted; volatile double total; };
static struct Wide wide;
static long long observations[27];
static unsigned char storage[40];
static int initialization_calls;
static int place_selections;
static int capture_int(int value) { ++initialization_calls; return value; }
static double capture_double(double value) { ++initialization_calls; return value; }
static struct Wide *selected_place(void) { ++place_selections; return &wide; }

int fixture(void) {
    initialization_calls = 0; place_selections = 0;
    unsigned char offset8 = 2;
    unsigned short offset16 = 1;
    unsigned int offset32 = 3;
    unsigned long offset64 = 4;
    long long signed64 = 2;
    long signed_long = 1;
    int signed32 = 1;
    unsigned char *pointer = storage;
    pointer += offset8; observations[0] = pointer - storage;
    pointer -= offset16; observations[1] = pointer - storage;
    pointer += offset32; observations[2] = pointer - storage;
    pointer += offset64; observations[3] = pointer - storage;
    pointer += signed64; observations[4] = pointer - storage;
    pointer -= signed_long; observations[5] = pointer - storage;
    pointer += signed32; observations[6] = pointer - storage;
    pointer += signed_long; observations[7] = pointer - storage;
    unsigned int narrow_shift = 3;
    narrow_shift <<= 5; observations[8] = narrow_shift;
    narrow_shift >>= 4; observations[9] = narrow_shift;
    unsigned long long wide_shift = 3;
    long long wide_count = 33;
    wide_shift <<= wide_count; observations[10] = (long long)wide_shift;
    wide_shift >>= wide_count; observations[11] = (long long)wide_shift;
    volatile int constant_initialization = -7;
    volatile int evaluated_initialization = capture_int(11);
    volatile double anchor = capture_double(9007199254740992.0);
    volatile double rounded = anchor + 1.0;
    volatile double next = anchor + 2.0;
    observations[12] = constant_initialization;
    observations[13] = evaluated_initialization;
    observations[14] = (long long)(rounded - anchor);
    observations[15] = (long long)(next - anchor);
    wide.interrupted = -17;
    wide.total = 0.5;
    selected_place()->total += 1.25;
    observations[16] = wide.interrupted;
    observations[17] = (long long)(wide.total * 4.0);
    observations[18] = initialization_calls;
    observations[19] = place_selections;
    volatile int *address_only = &wide.interrupted;
    observations[20] = address_only == &wide.interrupted;
    double assigned = (wide.total = 2.5);
    observations[21] = (long long)(assigned * 4.0);
    observations[22] = (long long)(wide.total * 4.0);
    double updated = (selected_place()->total += 0.75);
    observations[23] = (long long)(updated * 4.0);
    observations[24] = (long long)(wide.total * 4.0);
    observations[25] = place_selections;
    observations[26] = sizeof(struct Wide);
    return 27;
}
int memory_width_count(void) { return 27; }
long long memory_width_observe(int index) { return observations[index]; }
