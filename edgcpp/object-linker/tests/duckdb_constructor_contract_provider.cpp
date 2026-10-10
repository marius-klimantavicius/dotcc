#include "duckdb_constructor_contract.h"

// The shared RTTI closure owns deleting destructors. Match their actual
// allocation ABI; these are real deallocation implementations, not stubs.
extern "C" void free(void *);
void operator delete(void *storage) noexcept { free(storage); }
void operator delete(void *storage, __SIZE_TYPE__) noexcept { free(storage); }
static bool should_throw;
int constructor_contract_should_throw() { return should_throw; }
ConstructorContract::ConstructorContract() noexcept : own(1) {}
int main(int argc, char **) {
    should_throw = argc > 1;
    alignas(ConstructorContract) unsigned char storage[sizeof(ConstructorContract)];
    try {
        auto p = construct_contract(storage);
        int result = p->value + p->own;
        p->~ConstructorContract();
        return result == 42 ? 0 : 1;
    } catch (...) {
        // Reaching this handler means the complete constructor failed to
        // enforce noexcept around its virtual-base initializer.
        return 7;
    }
}
