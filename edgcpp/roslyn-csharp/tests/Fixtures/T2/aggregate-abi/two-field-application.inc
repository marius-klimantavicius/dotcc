/* Application implementation, genuinely preprocessed/exported for each shape.
   This file must NEVER be linked into a generated workload as native code. */
static ABI_TYPE transform(ABI_TYPE value) {
    value.first += ABI_FIRST_DELTA;
    value.second += ABI_SECOND_DELTA;
    return value;
}
static ABI_CALLBACK callback = transform;
static int observations[20];
int aggregate_observation_count(void) { return 20; }
int aggregate_observe(int selector) { return observations[selector]; }
int fixture(void) {
    ABI_TYPE original = {ABI_FIRST, ABI_SECOND};
    ABI_TYPE direct = transform(original);
    ABI_TYPE indirect = callback(original);
    ABI_TYPE echoed = ABI_ECHO(original);
    ABI_TYPE reentered = ABI_INVOKE(callback, original);
    ABI_CALLBACK supplied = ABI_PROVIDER();
    ABI_TYPE unknown = supplied(original);
    observations[0] = (int)(direct.first * ABI_SCALE);
    observations[1] = (int)(direct.second * ABI_SCALE);
    observations[2] = (int)(indirect.first * ABI_SCALE);
    observations[3] = (int)(indirect.second * ABI_SCALE);
    observations[4] = (int)(echoed.first * ABI_SCALE);
    observations[5] = (int)(echoed.second * ABI_SCALE);
    observations[6] = (int)(reentered.first * ABI_SCALE);
    observations[7] = (int)(reentered.second * ABI_SCALE);
    observations[8] = (int)(unknown.first * ABI_SCALE);
    observations[9] = (int)(unknown.second * ABI_SCALE);
    observations[10] = (int)(ABI_FIELD(&reentered, 0) * ABI_SCALE);
    observations[11] = (int)(ABI_FIELD(&reentered, 1) * ABI_SCALE);
    observations[12] = ABI_ECHO_CALLBACK(callback) == callback;
    observations[13] = ABI_PROVIDER() == supplied;
    observations[14] = ABI_FIELD(&echoed, 0) == ABI_FIRST;
    observations[15] = ABI_FIELD(&echoed, 1) == ABI_SECOND;
    observations[16] = ABI_FIELD(&unknown, 0) == ABI_FIRST;
    observations[17] = ABI_FIELD(&unknown, 1) == ABI_SECOND;
    observations[18] = ABI_FIELD(&reentered, 0) == ABI_FIRST + ABI_FIRST_DELTA;
    observations[19] = ABI_FIELD(&reentered, 1) == ABI_SECOND + ABI_SECOND_DELTA;
    int result = 0;
    for (int index = 0; index < 20; ++index) result += observations[index];
    return result;
}
