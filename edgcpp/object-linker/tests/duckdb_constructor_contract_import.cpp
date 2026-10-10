#include "duckdb_constructor_contract.h"
void *operator new(__SIZE_TYPE__, void *where) noexcept { return where; }
void operator delete(void *, void *) noexcept {}
ConstructorContract *construct_contract(void *where) {
    return new(where) ConstructorContract;
}
