int value = 10;
int *pointer = &value;
int *saved = nullptr;
unsigned long previous_address = 0;
alignas(64) long aligned = 27;
int check(int *p) { return *p; }
int main() {
    if (saved && saved != &value) return 101;
    if (pointer != &value) return 102;
    if (previous_address && previous_address != reinterpret_cast<unsigned long>(&value)) return 103;
    if (reinterpret_cast<unsigned long>(&aligned) % 64 != 0) return 104;
    saved = &value;
    previous_address = reinterpret_cast<unsigned long>(&value);
    // Managed tests collect concurrently with this translated call tree.
    int local = 7;
    int *p = &local;
    for (int i = 0; i < 200000; ++i) {
        if (check(p) != 7 || check(pointer) != value) return 105;
    }
    return ++*pointer;
}
