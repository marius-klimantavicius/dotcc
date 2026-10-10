// Reduction of ListLambdaBindData::Copy's unique_ptr-valued conditional.
int duckdb_conditional_record_destructions;
struct DuckdbConditionalRecord {
    int *value;
    constexpr DuckdbConditionalRecord(decltype(nullptr)) : value(nullptr) {}
    explicit DuckdbConditionalRecord(int *pointer) : value(pointer) {}
    DuckdbConditionalRecord(const DuckdbConditionalRecord &) = delete;
    DuckdbConditionalRecord &operator=(const DuckdbConditionalRecord &) = delete;
    DuckdbConditionalRecord(DuckdbConditionalRecord &&other) : value(other.value) {
        other.value = nullptr;
    }
    ~DuckdbConditionalRecord() { ++duckdb_conditional_record_destructions; }
};
DuckdbConditionalRecord duckdb_make_conditional_record(int *pointer) {
    return DuckdbConditionalRecord(pointer);
}
int duckdb_select_conditional_record(bool select) {
    int number = 42;
    auto result = select ? duckdb_make_conditional_record(&number) : nullptr;
    return select ? (result.value == &number && *result.value == 42 ? 0 : 1)
                  : (result.value == nullptr ? 0 : 2);
}
int main() {
    if (duckdb_select_conditional_record(true) || duckdb_select_conditional_record(false)) return 1;
    return duckdb_conditional_record_destructions == 2 ? 0 : 2;
}
