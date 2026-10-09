// DuckDB date.cpp has const string_t arrays initialized by runtime constructors.
// Keep this reduction independent of libc/libc++ and of nontrivial destruction.
int constructor_trace;
int constructor_count;

struct StringEntry {
    const char *text;
    unsigned length;
    char first;
    StringEntry(const char *value) : text(value), length(0), first(value[0]) {
        while (value[length]) ++length;
        constructor_trace = constructor_trace * 10 + int(length);
        ++constructor_count;
    }
};

const StringEntry names[] = {"Jan", "February", "March"};
int observed_after_array = constructor_count;

int main() {
    if (constructor_count != 3 || constructor_trace != 385 || observed_after_array != 3)
        return 1;
    if (names[0].length != 3 || names[0].first != 'J' || names[0].text[2] != 'n')
        return 2;
    if (names[1].length != 8 || names[1].first != 'F' || names[1].text[7] != 'y')
        return 3;
    if (names[2].length != 5 || names[2].first != 'M' || names[2].text[4] != 'h')
        return 4;
    return 0;
}
