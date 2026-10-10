// libc++ shared_ptr's constexpr default constructor is folded to a constant
// aggregate before add_init_assignment. Initializing it is not a copy.
void *operator new(unsigned long, void *storage) noexcept { return storage; }
void operator delete(void *, void *) noexcept {}
int duckdb_constant_anchor = 73;
struct DuckdbRecordConstant {
    int *first;
    int *second;
    constexpr DuckdbRecordConstant() : first(nullptr), second(nullptr) {}
    constexpr DuckdbRecordConstant(int *value) : first(value), second(&duckdb_constant_anchor) {}
    DuckdbRecordConstant(const DuckdbRecordConstant &) = delete;
    DuckdbRecordConstant(DuckdbRecordConstant &&other) : first(other.first), second(other.second) {
        other.first = other.second = nullptr;
    }
    ~DuckdbRecordConstant() {}
};
DuckdbRecordConstant make_constant(int *value) {
    DuckdbRecordConstant result;
    result.first = value;
    return result;
}
extern "C" int duckdb_record_initialization_constexpr() {
    int value = 31;
    DuckdbRecordConstant result = make_constant(&value);
    DuckdbRecordConstant relocated(&duckdb_constant_anchor);
    // ICU UMutex::getMutex uses placement new of a constexpr mutex. The
    // internal allocation-failure boundary discards the initialization result;
    // it does not copy the noncopyable record out of its new storage.
    alignas(DuckdbRecordConstant) unsigned char storage[sizeof(DuckdbRecordConstant)];
    auto placed = new(storage) DuckdbRecordConstant(&duckdb_constant_anchor);
    bool placed_valid = placed->first == &duckdb_constant_anchor &&
        placed->second == &duckdb_constant_anchor;
    placed->~DuckdbRecordConstant();
    return placed_valid && result.first == &value && result.second == nullptr &&
        relocated.first == &duckdb_constant_anchor && relocated.second == &duckdb_constant_anchor &&
        *relocated.first == 73 ? 0 : 1;
}
#ifdef DUCKDB_RECORD_NATIVE_ORACLE
int main() { return duckdb_record_initialization_constexpr(); }
#endif
