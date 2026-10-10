// Force different generated anonymous C tag counters in this TU before the
// shared declarations; these unrelated types must not affect shared identity.
template<class T> struct DuckdbAnonymousNoise { union { T number; double other; }; };
int duckdb_anonymous_noise(DuckdbAnonymousNoise<int> *value) { return value->number; }
#include "duckdb_anonymous_record_identity.h"
int duckdb_anonymous_consume(DuckdbAnonymousBox<int> *value) {
    return value->representation.value + value->representation.first.value + value->representation.second.value;
}
int duckdb_anonymous_long(DuckdbAnonymousBox<long> *value) {
    return int(value->representation.value) + value->representation.first.value + value->representation.second.value;
}
int duckdb_anonymous_typedef(DuckdbTypedefRecord *value) { return duckdb_anonymous_local(value->value); }
