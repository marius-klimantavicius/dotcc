//type: fn
//options: 
# 1 "CXX/basic/basic.stc/basic.stc.dynamic/basic.stc.dynamic.allocation/p1.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 482 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "CXX/basic/basic.stc/basic.stc.dynamic/basic.stc.dynamic.allocation/p1.cpp" 2

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
# 3 "CXX/basic/basic.stc/basic.stc.dynamic/basic.stc.dynamic.allocation/p1.cpp" 2

struct A {
  void *operator new(size_t);
};

namespace NS {
  void *operator new(size_t);;
}

static void *operator new(size_t);
static void *operator new(size_t, int, int);

struct B {
  void operator new(size_t);
};

struct C {
  void *operator new();
};

struct D {
  void *operator new(bool);
};

struct E {
  void *operator new(size_t = 0);
};

struct F {
  template<typename T> void *operator new(size_t, int);
};

struct G {
  template<typename T> T operator new(size_t, int);
};

struct H {
  template<typename T> void *operator new(T, int);
};

struct I {
  template<typename T> void *operator new(size_t);
};
