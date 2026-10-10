// Array registration selects an owned generated callback, not one element's dtor.
extern "C" int putchar(int);
struct RegistryEntry {
    int value;
    RegistryEntry(int v) : value(v) { putchar('0' + value); }
    ~RegistryEntry() { putchar('4' + value); }
};
RegistryEntry registry_entries[2] = {1, 2};
int main() {
    if (registry_entries[0].value != 1 || registry_entries[1].value != 2) return 1;
    putchar('0');
    return 0;
}
