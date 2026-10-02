//type: fn
//options:  --ms_compatibility --c++11
# 1 "SemaCXX/microsoft-new-delete.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/microsoft-new-delete.cpp" 2


# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 1
# 84 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_header_macro.h" 1
# 85 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2



# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_ptrdiff_t.h" 1
# 18 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_ptrdiff_t.h"
typedef long int ptrdiff_t;
# 89 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_size_t.h" 1
# 18 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_size_t.h"
typedef long unsigned int size_t;
# 94 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2
# 103 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_wchar_t.h" 1
# 104 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_null.h" 1
# 109 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_nullptr_t.h" 1
# 114 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2
# 123 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h"
# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_max_align_t.h" 1
# 19 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_max_align_t.h"
typedef struct {
  long long __clang_max_align_nonce1
      __attribute__((__aligned__(__alignof__(long long))));
  long double __clang_max_align_nonce2
      __attribute__((__aligned__(__alignof__(long double))));
} max_align_t;
# 124 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2




# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/__stddef_offsetof.h" 1
# 129 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stddef.h" 2
# 4 "SemaCXX/microsoft-new-delete.cpp" 2

struct arbitrary_t {} arbitrary;
void *operator new(size_t size, arbitrary_t);

void f() {

  int *p = new(arbitrary) int[4];
}

class noncopyable { noncopyable(const noncopyable&); } extern nc;
void *operator new[](size_t, noncopyable);
void *operator new(size_t, const noncopyable&);
void *q = new (nc) int[4];

struct bitfield { int n : 3; } bf;
void *operator new[](size_t, int &);
void *operator new(size_t, const int &);
void *r = new (bf.n) int[4];

struct base {};
struct derived : private base {} der;
void *operator new[](size_t, base &);
void *operator new(size_t, derived &);
void *s = new (der) int[4];

struct explicit_ctor { explicit explicit_ctor(int); };
struct explicit_ctor_tag {} ect;
void *operator new[](size_t, explicit_ctor_tag, explicit_ctor);
void *operator new(size_t, explicit_ctor_tag, int);
void *t = new (ect, 0) int[4];
void *u = new (ect, {0}) int[4];
