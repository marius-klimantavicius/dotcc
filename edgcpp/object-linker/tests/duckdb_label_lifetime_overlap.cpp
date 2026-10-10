static int trace;
struct LabelGuard {
    int value;
    LabelGuard(int v) noexcept : value(v) { trace = trace * 10 + value; }
    ~LabelGuard() noexcept { trace = trace * 10 + value; }
};
static void run(bool fail) {
    LabelGuard outer(1);
    int visits = 0;
again:
    LabelGuard inner(2);
    if (fail) throw 17;
    if (visits++ == 0) goto again;

}
int main() {
    run(false);
    if (trace != 122221) return 1;
    trace = 0;
    try { run(true); }
    catch (int value) { return value == 17 && trace == 1221 ? 0 : 2; }
    return 3;
}

// Shared RTTI deleting-destructor closure uses the ordinary allocation ABI.
extern "C" void free(void *);
void operator delete(void *storage) noexcept { free(storage); }
void operator delete(void *storage, __SIZE_TYPE__) noexcept { free(storage); }
