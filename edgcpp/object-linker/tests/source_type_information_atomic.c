struct AtomicCatalog {
    _Atomic(int) counter;
    const volatile _Atomic(unsigned int)* flags;
    int* restrict restricted_pointer;
};

int inspect_atomic(const struct AtomicCatalog* value) { return value == 0 ? 0 : 1; }
