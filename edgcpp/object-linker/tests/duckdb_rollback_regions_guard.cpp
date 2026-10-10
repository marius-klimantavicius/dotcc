static int attempts;
static int concurrent_attempts;
static volatile unsigned int publication_work;
static int initialize(int fail) {
    ++attempts;
    if (fail) throw 8;
    return 42;
}
extern "C" int duckdb_rollback_guard(int fail) {
    try {
        static int value = initialize(fail);
        return value;
    } catch (int error) { return -error; }
}
extern "C" int duckdb_rollback_guard_attempts() { return attempts; }

struct Published {
    unsigned long long first;
    unsigned long long second;
    unsigned long long third;
};
static Published initialize_concurrent() {
    ++concurrent_attempts;
    // The standard guard owns this increment and all writes to the object.
    // After the first attempt aborts, exactly one waiting caller may retry.
    if (concurrent_attempts == 1) throw 8;
    Published result;
    result.first = 0x1122334455667788ULL;
    for (unsigned int i = 0; i != 200000; ++i) publication_work = i;
    result.second = 0x8877665544332211ULL;
    result.third = result.first ^ result.second;
    return result;
}
extern "C" int duckdb_rollback_guard_concurrent() {
    try {
        static Published value = initialize_concurrent();
        return value.first == 0x1122334455667788ULL && value.second == 0x8877665544332211ULL &&
            value.third == (value.first ^ value.second) ? 42 : -99;
    } catch (int error) { return -error; }
}
extern "C" int duckdb_rollback_guard_concurrent_attempts() { return concurrent_attempts; }
