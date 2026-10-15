/* Genuine unsupported-admission input. No native application provider is used. */
typedef int AlignedInt __attribute__((aligned(16)));

static int consume(AlignedInt prefix, ...) {
    __builtin_va_list cursor;
    __builtin_va_start(cursor, prefix);
    int result = prefix + __builtin_va_arg(cursor, int);
    __builtin_va_end(cursor);
    return result;
}

int fixture(void) {
    AlignedInt prefix = 1;
    return consume(prefix, 2);
}
