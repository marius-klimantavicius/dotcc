static int union_destructor_total;
union DuckdbRecordUnion {
    int integer;
    double real;
    DuckdbRecordUnion(const DuckdbRecordUnion &) = delete;
    DuckdbRecordUnion &operator=(const DuckdbRecordUnion &) = delete;
    ~DuckdbRecordUnion() { union_destructor_total += integer; }
};
extern "C" int duckdb_record_initialization_union() {
    union_destructor_total = 0;
    { DuckdbRecordUnion value{19}; if (value.integer != 19) return 1; }
    return union_destructor_total == 19 ? 0 : 2;
}
#ifdef DUCKDB_RECORD_NATIVE_ORACLE
int main() { return duckdb_record_initialization_union(); }
#endif
