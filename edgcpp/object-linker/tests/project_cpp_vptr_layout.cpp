struct VptrLayoutRecord {
    virtual int operation() const;
    int value;
};
VptrLayoutRecord *record_pointer;
extern "C" int read_record_value(const VptrLayoutRecord *record) {
    return record->value;
}
extern "C" unsigned long record_size() { return sizeof(VptrLayoutRecord); }
extern "C" unsigned long record_alignment() { return alignof(VptrLayoutRecord); }
int main() {
    return sizeof(VptrLayoutRecord) != 16 || alignof(VptrLayoutRecord) != 8 || record_pointer != nullptr;
}
