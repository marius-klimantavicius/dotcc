//type: fn
//options:  --c++11: --c++11
# 1 "SemaCXX/conversion.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/conversion.cpp" 2



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
# 5 "SemaCXX/conversion.cpp" 2

typedef signed char int8_t;
typedef signed short int16_t;
typedef signed int int32_t;
typedef signed long int64_t;

typedef unsigned char uint8_t;
typedef unsigned short uint16_t;
typedef unsigned int uint32_t;
typedef unsigned long uint64_t;

namespace test0 {
  int32_t test1_positive(char *I, char *E) {
    return (E - I);
  }

  int32_t test1_negative(char *I, char *E) {
    return static_cast<int32_t>(E - I);
  }

  uint32_t test2_positive(uint64_t x) {
    return x;
  }

  uint32_t test2_negative(uint64_t x) {
    return (uint32_t) x;
  }
}

namespace test1 {
  uint64_t test1(int x, unsigned y) {
    return sizeof(x == y);
  }

  uint64_t test2(int x, unsigned y) {
    return __alignof(x == y);
  }

  void * const foo();
  bool test2(void *p) {
    return p == foo();
  }
}

namespace test2 {
  struct A {
    unsigned int x : 2;
    A() : x(10) {}
  };
}




void test3() {
  int a = __null;
  int b;
  b = __null;
  long l = __null;
  int c = ((((__null))));
  int d;
  d = ((((__null))));
  bool bl = __null;
  char ch = __null;
  unsigned char uch = __null;
  short sh = __null;
  double dbl = __null;






  int a3 = __null;



  int *ip = __null;
  int (*fp)() = __null;
  struct foo {
    int n;
    void func();
  };
  int foo::*datamem = __null;
  int (foo::*funmem)() = __null;
}

namespace test4 {


  template<typename T>
  void tmpl(char c = __null,
            T a = __null,

            T b = 1024) {
  }

  template<typename T>
  void tmpl2(T t = __null) {
  }

  void func() {
    tmpl<char>();
    tmpl<int>();
    tmpl<int>();
    tmpl2<int*>();
  }
}

namespace test5 {
  template<int I>
  void func() {
    bool b = I;
  }

  template void func<3>();
}

namespace test6 {
  decltype(nullptr) func() {
    return __null;
  }
}

namespace test7 {
  bool fun() {
    bool x = nullptr;
    if (nullptr) {}
    return nullptr;
  }
}

namespace test8 {





  void macro() {
    int num;
    bool b = ((true) ? &num : __null);
    if (((true) ? &num : __null)) {}
    while (((true) ? &num : __null)) {}
    for (;((true) ? &num : __null);) {}
    do {} while (((true) ? &num : __null));

    if (((false) ? &num : __null)) {}
    while (((false) ? &num : __null)) {}
    for (;((false) ? &num : __null);) {}
    do {} while (((false) ? &num : __null));
  }




  template <typename X>
  void template_and_macro() {
    int num;
    bool b = ((true) ? &num : __null);
    if (((true) ? &num : __null)) {}
    while (((true) ? &num : __null)) {}
    for (;((true) ? &num : __null);) {}
    do {} while (((true) ? &num : __null));

    if (((false) ? &num : __null)) {}
    while (((false) ? &num : __null)) {}
    for (;((false) ? &num : __null);) {}
    do {} while (((false) ? &num : __null));
  }



  template <typename X>
  void template_and_macro2() {
    X num;
    bool b = ((true) ? &num : __null);
    if (((true) ? &num : __null)) {}
    while (((true) ? &num : __null)) {}
    for (;((true) ? &num : __null);) {}
    do {} while (((true) ? &num : __null));

    if (((false) ? &num : __null)) {}
    while (((false) ? &num : __null)) {}
    for (;((false) ? &num : __null);) {}
    do {} while (((false) ? &num : __null));
  }

  void run() {
    template_and_macro<int>();
    template_and_macro<double>();
    template_and_macro2<int>();
    template_and_macro2<double>();
  }
}

namespace test9 {
  typedef decltype(nullptr) nullptr_t;
  nullptr_t EXIT();

  bool test() {
    return EXIT();
  }
}


namespace test10 {







  void foo();
  void bar();

  void run(int x) {
    if (((x) ? nullptr : __null)) {}
    if (((x) ? __null : nullptr)) {}
    ((((x) ? nullptr : __null)) ? foo() : bar());
    ((((x) ? __null : nullptr)) ? foo() : bar());
  }
}

namespace test11 {
# 238 "SemaCXX/conversion.cpp"
int dostuff ();

void test(const char * content_type) {
  (((dostuff() ? __null : __null)) ? 0 : 0);
  (((dostuff() ? __null : __null)) ? 0 : 0);
}

}

namespace test12 {



bool run() {
  return __null;
}

}





namespace test13 {






void function1(const char* str) {
  if (!(((str) ? str : nullptr))) return;;
  if (!(((str) ? str : __null))) return;;
}

bool some_bool_function(bool);
void function2() {
  if (!(some_bool_function(nullptr))) return;;
  if (!(some_bool_function(__null))) return;;
}





void function3(const char* str) {
  if (((str) ? str : nullptr)) return;
  if (((str) ? str : __null)) return;
  if (((str) ? str : nullptr)) return;
  if (((str) ? str : __null)) return;
}

void run(int* ptr);


void function4() {
  if (nullptr) run(nullptr);;
  if (__null) run(__null);;
}
}
