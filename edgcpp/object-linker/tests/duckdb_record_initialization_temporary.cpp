// A constexpr class-valued delegating constructor argument has no source local.
int duckdb_temporary_record_destructions;
struct DuckdbTemporaryRecord {
    int *pointer;
    constexpr DuckdbTemporaryRecord(decltype(nullptr)) : pointer(nullptr) {}
    DuckdbTemporaryRecord(const DuckdbTemporaryRecord &) = delete;
    DuckdbTemporaryRecord &operator=(const DuckdbTemporaryRecord &) = delete;
    DuckdbTemporaryRecord(DuckdbTemporaryRecord &&other) : pointer(other.pointer) { other.pointer = nullptr; }
    DuckdbTemporaryRecord &operator=(DuckdbTemporaryRecord &&other) { pointer = other.pointer; other.pointer = nullptr; return *this; }
    ~DuckdbTemporaryRecord() { ++duckdb_temporary_record_destructions; }
};
struct DuckdbDerivedTemporaryRecord : DuckdbTemporaryRecord {
    using DuckdbTemporaryRecord::DuckdbTemporaryRecord;
};
struct DuckdbTemporaryOwner {
    int *pointer;
#ifdef DUCKDB_RECORD_TWO_TEMPORARIES
    explicit DuckdbTemporaryOwner(DuckdbDerivedTemporaryRecord value, DuckdbDerivedTemporaryRecord second)
        : pointer(value.pointer == second.pointer ? value.pointer : (int *)1) {}
    DuckdbTemporaryOwner() : DuckdbTemporaryOwner(nullptr, nullptr) {}
#else
    explicit DuckdbTemporaryOwner(DuckdbDerivedTemporaryRecord value) : pointer(value.pointer) {}
    DuckdbTemporaryOwner() : DuckdbTemporaryOwner(nullptr) {}
#endif
};
int main() {
    DuckdbTemporaryOwner value;
#ifdef DUCKDB_RECORD_TWO_TEMPORARIES
    return value.pointer == nullptr && duckdb_temporary_record_destructions == 2 ? 0 : 1;
#else
    return value.pointer == nullptr && duckdb_temporary_record_destructions == 1 ? 0 : 1;
#endif
}
