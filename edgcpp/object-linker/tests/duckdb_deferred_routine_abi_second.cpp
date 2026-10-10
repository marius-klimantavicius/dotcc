struct DeferredResult {
    int value;
    explicit DeferredResult(int v) : value(v) {}
    DeferredResult(const DeferredResult &other) : value(other.value) {}
};
#include "duckdb_deferred_routine_abi.h"
DeferredResult make_result(int value) { return DeferredResult(value); }
int read_value(int value) { return value + 4; }
int main() {
    DeferredCallbacks callbacks{make_result, read_value, {make_result, make_result}, 0};
    if (inspect_callbacks(&callbacks) != 17) return 1;
    DeferredResult result = callbacks.make(27);
    if (result.value != 27) return 2;
    callbacks.indirect = &callbacks.extras[1];
    DeferredResult second = (*callbacks.indirect)(31);
    return second.value == 31 ? 0 : 3;
}
