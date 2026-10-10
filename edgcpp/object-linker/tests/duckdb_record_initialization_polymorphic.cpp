// The implicit constructor lowers to a constant aggregate of vptrs. This is
// initialization of one object's storage, never a copy from a zeroed object.
struct DuckdbRecordPolymorphic {
    virtual int value() const { return 17; }
};
extern "C" int duckdb_record_initialization_polymorphic() {
    DuckdbRecordPolymorphic value;
    return value.value() == 17 ? 0 : 1;
}
#ifdef DUCKDB_RECORD_NATIVE_ORACLE
int main() { return duckdb_record_initialization_polymorphic(); }
#endif
