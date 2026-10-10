// A real program-supplied global delete replacement. This test controls the
// matching allocation family; it does not stand in for a complete libc++ heap.
typedef __SIZE_TYPE__ size_t;
extern "C" void dotobj_test_free(void *);
void operator delete(void *pointer) noexcept { dotobj_test_free(pointer); }
void operator delete(void *pointer, size_t) noexcept { ::operator delete(pointer); }
extern "C" void array_delete_adapter_probe(void *pointer) {
    ::operator delete(pointer, size_t(13));
}
