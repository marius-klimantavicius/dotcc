// Exercise the actual vector allocation protocol using source-defined
// allocation callbacks. No host runtime stubs or external heap are needed.
extern "C" int putchar(int);
alignas(32) static unsigned char vector_arena[256];
static unsigned long vector_requested_bytes, vector_requested_elements;
static int vector_allocator_calls, vector_deallocator_calls;
static int vector_constructor_calls, vector_destructor_calls;
static int vector_next_id, vector_fail_constructor;
static bool vector_fail_allocator;
static int vector_errors;

struct DuckdbVectorElement {
    int id;
    DuckdbVectorElement();
    ~DuckdbVectorElement() {
        ++vector_destructor_calls;
        putchar('a' + id - 1);
    }
    static void* operator new[](unsigned long bytes) {
        ++vector_allocator_calls;
        vector_requested_bytes = bytes;
        putchar('A');
        if (vector_fail_allocator || bytes > sizeof(vector_arena)) throw 9;
        return vector_arena;
    }
#ifdef DUCKDB_VECTOR_SIZED_DELETE
    static void operator delete[](void* memory, unsigned long bytes) noexcept {
        // IA64 sized deletion uses element extent plus the count cookie.
        // Stock EDG may round a zero payload to one byte when allocating;
        // that extra allocation granularity is not a constructed element.
        unsigned long expected = vector_requested_elements * sizeof(DuckdbVectorElement) + sizeof(unsigned long);
        if (bytes != expected || vector_requested_bytes < expected ||
            vector_requested_bytes > expected + (vector_requested_elements == 0 ? 1 : 0))
            ++vector_errors;
#else
    static void operator delete[](void* memory) noexcept {
#endif
        if (memory != vector_arena) ++vector_errors;
        ++vector_deallocator_calls;
        putchar('D');
    }
};

static DuckdbVectorElement* vector_allocate(unsigned long count) {
    vector_requested_elements = count;
    return new DuckdbVectorElement[count];
}

// Deliberately define this after vector_allocate: EDG builds the runtime
// callback address before lowering the out-of-line constructor receiver ABI.
DuckdbVectorElement::DuckdbVectorElement() : id(++vector_next_id) {
    ++vector_constructor_calls;
    putchar('0' + id);
    if (id == vector_fail_constructor) throw id;
}

static void vector_release(DuckdbVectorElement* memory, unsigned long count) {
#ifdef DUCKDB_VECTOR_ALLOCATION_ONLY
    // Isolate allocation-protocol ownership from the separate vector-delete
    // exporter feature. These remain ordinary source C++ calls, not host stubs.
    while (count) memory[--count].~DuckdbVectorElement();
#ifdef DUCKDB_VECTOR_SIZED_DELETE
    DuckdbVectorElement::operator delete[](vector_arena,
        vector_requested_elements * sizeof(DuckdbVectorElement) + sizeof(unsigned long));
#else
    DuckdbVectorElement::operator delete[](vector_arena);
#endif
#else
    (void)count;
    delete[] memory;
#endif
}

int main() {
    vector_release(vector_allocate(0), 0);
    vector_release(vector_allocate(2), 2);

    vector_fail_allocator = true;
    try { (void)vector_allocate(3); } catch (int value) {
        if (value != 9) ++vector_errors;
        putchar('!');
    }
    if (vector_deallocator_calls != 2) ++vector_errors;

    vector_fail_allocator = false;
    vector_next_id = 0;
    vector_fail_constructor = 2;
    try { (void)vector_allocate(3); } catch (int value) {
        if (value != 2) ++vector_errors;
        putchar('!');
    }
    if (vector_allocator_calls != 4 || vector_deallocator_calls != 3 ||
        vector_constructor_calls != 4 || vector_destructor_calls != 3)
        ++vector_errors;
    return vector_errors;
}
