#ifndef DUCKDB_DEFERRED_ROUTINE_ABI_H
#define DUCKDB_DEFERRED_ROUTINE_ABI_H
struct DeferredResult;
typedef DeferredResult (*DeferredFactory)(int);
struct DeferredCallbacks {
    DeferredFactory make;
    int (*read)(int);
    DeferredFactory extras[2];
    DeferredFactory *indirect;
};
int inspect_callbacks(DeferredCallbacks *callbacks);
#endif
