struct OriginalRecord {
    unsigned bits : 3;
    unsigned : 0;
    mutable int value;
};

int update_original(const OriginalRecord *record) {
    record->value += 4;
    return record->value + record->bits;
}

int main() {
    OriginalRecord first = {5, 7};
    OriginalRecord second = first;
    if (update_original(&second) != 16) return 1;
    if (first.value != 7 || second.value != 11) return 2;
    return sizeof(OriginalRecord) != 8 || alignof(OriginalRecord) != 4;
}
