int local_constructor_count;
int argument_calls;

struct LocalEntry {
    int value;
    LocalEntry(int initial) : value(initial) { ++local_constructor_count; }
};

int next_argument() { return ++argument_calls + 40; }

LocalEntry &get_local() {
    static LocalEntry entry(next_argument());
    return entry;
}

int main() {
    if (local_constructor_count || argument_calls) return 1;
    LocalEntry *first = &get_local();
    if (local_constructor_count != 1 || argument_calls != 1 || first->value != 41)
        return 2;
    first->value = 87;
    LocalEntry *second = &get_local();
    if (first != second || second->value != 87 || local_constructor_count != 1 || argument_calls != 1)
        return 3;
    return 0;
}
