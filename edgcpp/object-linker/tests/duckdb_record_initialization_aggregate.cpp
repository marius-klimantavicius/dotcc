// C++17 aggregates may have deleted, non-user-provided copy constructors.
// These deleted operations make an invented record copy observable at compile
// time. Const/reference fields can be initialized but cannot later be assigned.
static int aggregate_destructor_total;
struct DuckdbRecordAggregate {
    const int fixed;
    int &alias;
    int values[3];
    DuckdbRecordAggregate(const DuckdbRecordAggregate &) = delete;
    DuckdbRecordAggregate &operator=(const DuckdbRecordAggregate &) = delete;
    ~DuckdbRecordAggregate() { aggregate_destructor_total += fixed + alias + values[0] + values[1] + values[2]; }
};
extern "C" int duckdb_record_initialization_aggregate() {
    aggregate_destructor_total = 0;
    int referred = 5;
    {
        DuckdbRecordAggregate value{3, referred, {7}};
        referred = 11;
        if (value.fixed != 3 || &value.alias != &referred || value.values[0] != 7 ||
            value.values[1] != 0 || value.values[2] != 0) return 1;
    }
    return aggregate_destructor_total == 21 ? 0 : 2;
}
#ifdef DUCKDB_RECORD_NATIVE_ORACLE
int main() { return duckdb_record_initialization_aggregate(); }
#endif
