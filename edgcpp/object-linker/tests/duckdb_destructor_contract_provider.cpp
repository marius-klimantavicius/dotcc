#include "duckdb_destructor_contract.h"
extern "C" void free(void *);
void operator delete(void *storage) noexcept { free(storage); }
void operator delete(void *storage, __SIZE_TYPE__) noexcept { free(storage); }
static bool should_throw;
int destructor_contract_should_throw() { return should_throw; }
DestructorContract::~DestructorContract() noexcept {}
int main(int argc, char **) {
    should_throw = argc > 1;
    alignas(DestructorContract) unsigned char storage[sizeof(DestructorContract)];
    try {
        auto p = new(storage) DestructorContract;
        int result = p->value + p->own;
        destroy_contract(p);
        return result == 42 ? 0 : 1;
    } catch (...) {
        // The virtual-base destruction runs in D1 after D2 returns. Escaping
        // to this catch means the complete destructor lost its own noexcept.
        return 7;
    }
}
