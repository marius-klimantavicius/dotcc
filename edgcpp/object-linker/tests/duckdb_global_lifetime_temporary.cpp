// Full-expression temporary ends during startup; global object ends after main.
extern "C" int putchar(int);
struct Temporary {
    int value;
    Temporary() : value(9) { putchar('1'); }
    ~Temporary() { putchar('2'); }
};
struct RegistryEntry {
    int value;
    explicit RegistryEntry(const Temporary &source) : value(source.value) { putchar('3'); }
    ~RegistryEntry() { putchar('4'); }
};
RegistryEntry registry_entry{Temporary()};
int main() {
    if (registry_entry.value != 9) return 1;
    putchar('0');
    return 0;
}
