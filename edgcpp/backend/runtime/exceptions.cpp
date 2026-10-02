/*
Exception-class bodies adapted from EDG lib_src/exception.c, typeinfo.c
and bad_alloc.c.
Part of the EDG Compiler Project.
SPDX-License-Identifier: Apache-2.0 WITH LLVM-exception
See https://edgcpp.org/LICENSE.txt for license information.
*/
#include <typeinfo>
#include <new>

// Storage and opaque RTTI names are the managed runtime bridge operations.
extern "C" void* __edg_dot_allocate(unsigned long) noexcept;
extern "C" void* __edg_dot_allocate_aligned(unsigned long,unsigned long) noexcept;
extern "C" void __edg_dot_free(void*) noexcept;
extern "C" const char* __edg_dot_type_name(const void*) noexcept;
extern "C" std::new_handler __edg_dot_set_handler(std::new_handler) noexcept;
extern "C" std::new_handler __edg_dot_get_handler() noexcept;
namespace std {
new_handler set_new_handler(new_handler value) noexcept {return __edg_dot_set_handler(value);}
new_handler get_new_handler() noexcept {return __edg_dot_get_handler();}
bool type_info::operator==(const type_info& rhs) const { return this==&rhs || name()==rhs.name(); }
bool type_info::operator!=(const type_info& rhs) const { return !(*this==rhs); }
bool type_info::before(const type_info& rhs) const { return name()<rhs.name(); }
const char* type_info::name() const { return __edg_dot_type_name(this); }

exception::exception() noexcept {}
exception::exception(const exception&) noexcept {}
exception& exception::operator=(const exception&) noexcept { return *this; }
exception::~exception() noexcept {}
const char* exception::what() const noexcept { return ""; }

bad_cast::bad_cast() noexcept {}
bad_cast::bad_cast(const bad_cast& r) noexcept : exception(r) {}
bad_cast& bad_cast::operator=(const bad_cast& r) noexcept { exception::operator=(r);return *this; }
bad_cast::~bad_cast() noexcept {}
const char* bad_cast::what() const noexcept { return ""; }

bad_typeid::bad_typeid() noexcept {}
bad_typeid::bad_typeid(const bad_typeid& r) noexcept : exception(r) {}
bad_typeid& bad_typeid::operator=(const bad_typeid& r) noexcept { exception::operator=(r);return *this; }
bad_typeid::~bad_typeid() noexcept {}
const char* bad_typeid::what() const noexcept { return ""; }

bad_alloc::bad_alloc() noexcept {}
bad_alloc::bad_alloc(const bad_alloc& r) noexcept : exception(r) {}
bad_alloc& bad_alloc::operator=(const bad_alloc& r) noexcept { exception::operator=(r);return *this; }
bad_alloc::~bad_alloc() noexcept {}
const char* bad_alloc::what() const noexcept { return ""; }
bad_array_new_length::bad_array_new_length() noexcept {}
bad_array_new_length::~bad_array_new_length() noexcept {}
}
static void* allocate(size_t n,size_t alignment) {
  for(;;) {
    void* p=__edg_dot_allocate_aligned(n?n:1,alignment);
    if(p)return p;
    auto handler=std::get_new_handler();
    if(!handler)throw std::bad_alloc();
    handler();
  }
}
void* operator new(size_t n) {return allocate(n,16);}
void operator delete(void* p) noexcept { __edg_dot_free(p); }
void operator delete(void* p,size_t) noexcept { operator delete(p); }
void* operator new[](size_t n) {return operator new(n);}
void operator delete[](void* p) noexcept {operator delete(p);}
void operator delete[](void* p,size_t) noexcept {operator delete[](p);}
void* operator new(size_t n,const std::nothrow_t&) noexcept {try{return operator new(n);}catch(...){return nullptr;}}
void* operator new[](size_t n,const std::nothrow_t&) noexcept {try{return operator new[](n);}catch(...){return nullptr;}}
void operator delete(void* p,const std::nothrow_t&) noexcept {operator delete(p);}
void operator delete[](void* p,const std::nothrow_t&) noexcept {operator delete[](p);}
void* operator new(size_t,void* p) noexcept {return p;}
void* operator new[](size_t,void* p) noexcept {return p;}
void operator delete(void*,void*) noexcept {}
void operator delete[](void*,void*) noexcept {}
void* operator new(size_t n,std::align_val_t a) {return allocate(n,(size_t)a);}
void* operator new[](size_t n,std::align_val_t a) {return operator new(n,a);}
void operator delete(void* p,std::align_val_t) noexcept {operator delete(p);}
void operator delete[](void* p,std::align_val_t) noexcept {operator delete[](p);}
void operator delete(void* p,size_t,std::align_val_t a) noexcept {operator delete(p,a);}
void operator delete[](void* p,size_t,std::align_val_t a) noexcept {operator delete[](p,a);}
void* operator new(size_t n,std::align_val_t a,const std::nothrow_t&) noexcept {try{return operator new(n,a);}catch(...){return nullptr;}}
void* operator new[](size_t n,std::align_val_t a,const std::nothrow_t&) noexcept {try{return operator new[](n,a);}catch(...){return nullptr;}}
void operator delete(void* p,std::align_val_t a,const std::nothrow_t&) noexcept {operator delete(p,a);}
void operator delete[](void* p,std::align_val_t a,const std::nothrow_t&) noexcept {operator delete[](p,a);}

// Keep C++ construction, EH type identity and destruction in ordinary lowered IL.
extern "C" [[noreturn]] void __edg_dot_throw_bad_cast() { throw std::bad_cast(); }
extern "C" [[noreturn]] void __edg_dot_throw_bad_typeid() { throw std::bad_typeid(); }
extern "C" [[noreturn]] void __edg_dot_throw_bad_array_new_length() {throw std::bad_array_new_length();}
