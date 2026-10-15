/* Genuine independently compiled C++ TU. Link by recorded identities only. */
namespace captured {
int choose(int value) { return value + 11; }
unsigned choose(unsigned value) { return value + 17u; }
}

static int local_helper(int value) { return value * 3; }

extern "C" int unit_a(int value) {
    return captured::choose(value) + (int)captured::choose((unsigned)value)
        + local_helper(value);
}
