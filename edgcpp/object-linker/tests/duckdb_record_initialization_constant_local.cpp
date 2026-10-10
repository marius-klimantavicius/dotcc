// Constant initialization of noncopyable storage, independent of NRVO cleanup.
int duckdb_local_constant_anchor = 73;
int duckdb_local_constant_destroyed;
struct DuckdbLocalConstant {
    int *first;
    int *second;
    int marker;
    constexpr DuckdbLocalConstant() : first(nullptr), second(nullptr), marker(0) {}
    constexpr DuckdbLocalConstant(int *value)
        : first(value), second(&duckdb_local_constant_anchor), marker(97) {}
    DuckdbLocalConstant(const DuckdbLocalConstant &) = delete;
    DuckdbLocalConstant &operator=(const DuckdbLocalConstant &) = delete;
    ~DuckdbLocalConstant() { ++duckdb_local_constant_destroyed; }
};
// C++17 constructs these prvalues directly in the caller-owned result buffer;
// there is no named local whose destruction needs an NRVO transfer.
DuckdbLocalConstant make_local_zero() { return DuckdbLocalConstant(); }
DuckdbLocalConstant make_local_relocated() {
    return DuckdbLocalConstant(&duckdb_local_constant_anchor);
}
extern "C" int duckdb_record_initialization_constant_local() {
    {
        DuckdbLocalConstant zero = make_local_zero();
        DuckdbLocalConstant relocated = make_local_relocated();
        if (zero.first || zero.second || zero.marker != 0 ||
            relocated.first != &duckdb_local_constant_anchor ||
            relocated.second != &duckdb_local_constant_anchor ||
            *relocated.first != 73 || relocated.marker != 97)
            return 1;
        if (duckdb_local_constant_destroyed != 0) return 2;
    }
    return duckdb_local_constant_destroyed == 2 ? 0 : 3;
}
#ifdef DUCKDB_RECORD_NATIVE_ORACLE
int main() { return duckdb_record_initialization_constant_local(); }
#endif
