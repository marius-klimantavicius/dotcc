#include "duckdb_anonymous_record_identity.h"
int duckdb_anonymous_child(DuckdbAnonymousChild *value) { return value->value; }
int main() {
    DuckdbAnonymousBox<int> value{{{3}, {5}, {7}}};
    DuckdbAnonymousBox<long> other{{{11}, {13}, {17}}};
    DuckdbTypedefRecord named{19};
    return duckdb_anonymous_consume(&value) == 15 && duckdb_anonymous_long(&other) == 41 &&
        duckdb_anonymous_typedef(&named) == 19 && duckdb_anonymous_child(&value.representation.first) == 5 &&
        duckdb_anonymous_local(23) == 23 ? 0 : 1;
}
