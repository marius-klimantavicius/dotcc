// The minimal RTTI provider retains its deleting-destructor entry. Supply the
// ordinary deallocator it references; the shared runtime binds free to the
// same allocator used by translated C/C++ allocations.
extern "C" void free(void *);
void operator delete(void *address, unsigned long) noexcept { free(address); }

struct RecordState {
    int value;
    explicit RecordState(int v) : value(v) {}
    explicit operator bool() const { return value != 0; }
};

struct DiscardedRecord : virtual RecordState {
    explicit DiscardedRecord(int v) : RecordState(v) {}
    DiscardedRecord(const DiscardedRecord &) = delete;
    DiscardedRecord &operator=(const DiscardedRecord &) = delete;
    ~DiscardedRecord() { value = -1; }
};

DiscardedRecord &next_record(DiscardedRecord *record, int *calls) {
    ++*calls;
    return *record;
}

int discarded_record_probe(DiscardedRecord *record, int *calls) {
    // Like string_util.cpp: getline returns an uncopyable stream reference,
    // and the conversion to bool uses the same returned object.
    if (next_record(record, calls)) {
        (void)next_record(record, calls);
        next_record(record, calls);
        return (next_record(record, calls), 17);
    }
    return (next_record(record, calls), 23);
}

int main() {
    int calls = 0;
    DiscardedRecord live(1);
    if (discarded_record_probe(&live, &calls) != 17 || calls != 4) return 1;
    DiscardedRecord empty(0);
    if (discarded_record_probe(&empty, &calls) != 23 || calls != 6) return 2;
    return live.value == 1 && empty.value == 0 ? 0 : 3;
}
