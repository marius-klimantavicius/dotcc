// Destructor-only constant initialization and reverse successful registration.
extern "C" int putchar(int);
struct ConstantEntry {
    int value;
    constexpr ConstantEntry(int v) : value(v) {}
    ~ConstantEntry() { putchar('7'); }
};
const ConstantEntry constant_entry(3);
struct RegistryEntry {
    int value;
    explicit RegistryEntry(int v) : value(v) { putchar('0' + value); }
    ~RegistryEntry() { putchar('4' + value); }
};
RegistryEntry first_entry(1);
RegistryEntry second_entry(2);
int main() {
    if (constant_entry.value + first_entry.value + second_entry.value != 6) return 1;
    putchar('0');
    return 0;
}
