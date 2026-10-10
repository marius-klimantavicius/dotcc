#include "duckdb_destructor_contract.h"
void *operator new(__SIZE_TYPE__, void *where) noexcept { return where; }
void operator delete(void *, void *) noexcept {}
void destroy_contract(DestructorContract *value) {
    value->~DestructorContract();
}
