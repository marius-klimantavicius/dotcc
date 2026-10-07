#include "variable_asm_type.h"
typedef unsigned long size_t;
void* operator new(size_t, void* p) noexcept { return p; }
void operator delete(void*, void*) noexcept {}
extern "C" {
    alignas(demo::Object) unsigned char object_storage[sizeof(demo::Object)] = {};
    int initialized_storage = 43;
    __thread int thread_storage = 17;
    int unused_asm_provider_storage = 101;
}
int constructions;
int destructions;
void construct_object() { new(object_storage) demo::Object(57); }
void destroy_object() { ((demo::Object*)object_storage)->~Object(); }
void* original_address() { return object_storage; }
namespace original { int value = 89; }
int source_spelling __asm__("renamed_storage_symbol") = 97;
