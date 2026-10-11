constexpr bool selected_query() {
    return __builtin_is_constant_evaluated();
}

constexpr int folded_marker = selected_query() ? 7 : 17;
int trace;

bool runtime_query() {
    return selected_query();
}

bool direct_query(bool enabled) {
    return enabled && __builtin_is_constant_evaluated();
}

int selected_path() {
    if (__builtin_is_constant_evaluated()) {
        trace = 9;
        return 99;
    }
    trace = 3;
    return 23;
}

int main() {
    static_assert(selected_query(), "compile_time_query");
    if (folded_marker != 7) return 1;
    if (runtime_query()) return 2;
    if (direct_query(true) || direct_query(false)) return 3;
    if (selected_path() != 23 || trace != 3) return 4;
    return 0;
}
