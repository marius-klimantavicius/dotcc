#ifndef DUCKDB_ANONYMOUS_RECORD_IDENTITY_H
#define DUCKDB_ANONYMOUS_RECORD_IDENTITY_H
// Anonymous members require their owning original declaration and template
// specialization; EDG's per-TU generated C tags (__C1 etc.) are not identities.
template<class T> struct DuckdbAnonymousBox {
    struct Representation {
        union { T value; unsigned char bytes[sizeof(T)]; };
        struct { int value; } first;
        struct { int value; } second;
        int unrelated;
        struct { T value; } *left, *right;
    } representation;
};
typedef struct { int value; } DuckdbTypedefRecord;
using DuckdbAnonymousChild = decltype(((DuckdbAnonymousBox<int> *)nullptr)->representation.first);
inline int duckdb_anonymous_local(int n) {
    struct Local { union { int value; long other; }; };
    Local local{{n}};
    return local.value;
}
int duckdb_anonymous_consume(DuckdbAnonymousBox<int> *value);
int duckdb_anonymous_long(DuckdbAnonymousBox<long> *value);
int duckdb_anonymous_typedef(DuckdbTypedefRecord *value);
#endif
