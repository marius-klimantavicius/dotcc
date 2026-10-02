//type: fn
//options:  --exceptions --exceptions --c++11
# 1 "SemaCXX/nullptr.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 452 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/nullptr.cpp" 2

# 1 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdint.h" 1
# 56 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdint.h"
# 1 "/usr/include/stdint.h" 1 3 4
# 25 "/usr/include/stdint.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
# 345 "/usr/include/features.h" 3 4
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 346 "/usr/include/features.h" 2 3 4
# 375 "/usr/include/features.h" 3 4
# 1 "/usr/include/sys/cdefs.h" 1 3 4
# 392 "/usr/include/sys/cdefs.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 393 "/usr/include/sys/cdefs.h" 2 3 4
# 376 "/usr/include/features.h" 2 3 4
# 399 "/usr/include/features.h" 3 4
# 1 "/usr/include/gnu/stubs.h" 1 3 4
# 10 "/usr/include/gnu/stubs.h" 3 4
# 1 "/usr/include/gnu/stubs-64.h" 1 3 4
# 11 "/usr/include/gnu/stubs.h" 2 3 4
# 400 "/usr/include/features.h" 2 3 4
# 26 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wchar.h" 1 3 4
# 22 "/usr/include/bits/wchar.h" 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 23 "/usr/include/bits/wchar.h" 2 3 4
# 27 "/usr/include/stdint.h" 2 3 4
# 1 "/usr/include/bits/wordsize.h" 1 3 4
# 28 "/usr/include/stdint.h" 2 3 4








typedef signed char int8_t;
typedef short int int16_t;
typedef int int32_t;

typedef long int int64_t;







typedef unsigned char uint8_t;
typedef unsigned short int uint16_t;

typedef unsigned int uint32_t;



typedef unsigned long int uint64_t;
# 65 "/usr/include/stdint.h" 3 4
typedef signed char int_least8_t;
typedef short int int_least16_t;
typedef int int_least32_t;

typedef long int int_least64_t;






typedef unsigned char uint_least8_t;
typedef unsigned short int uint_least16_t;
typedef unsigned int uint_least32_t;

typedef unsigned long int uint_least64_t;
# 90 "/usr/include/stdint.h" 3 4
typedef signed char int_fast8_t;

typedef long int int_fast16_t;
typedef long int int_fast32_t;
typedef long int int_fast64_t;
# 103 "/usr/include/stdint.h" 3 4
typedef unsigned char uint_fast8_t;

typedef unsigned long int uint_fast16_t;
typedef unsigned long int uint_fast32_t;
typedef unsigned long int uint_fast64_t;
# 119 "/usr/include/stdint.h" 3 4
typedef long int intptr_t;


typedef unsigned long int uintptr_t;
# 134 "/usr/include/stdint.h" 3 4
typedef long int intmax_t;
typedef unsigned long int uintmax_t;
# 57 "/mds/clang/clang-22.1.0/build/lib/clang/22/include/stdint.h" 2
# 3 "SemaCXX/nullptr.cpp" 2

typedef decltype(nullptr) nullptr_t;

struct A {};

int o1(char*);
void o1(uintptr_t);
void o2(char*);
void o2(int A::*);

nullptr_t f(nullptr_t null)
{

  null = nullptr;
  void *p = nullptr;
  p = null;
  int *pi = nullptr;
  pi = null;
  null = 0;
  int A::*pm = nullptr;
  pm = null;
  void (*pf)() = nullptr;
  pf = null;
  void (A::*pmf)() = nullptr;
  pmf = null;
  bool b = nullptr;


  uintptr_t i = nullptr;


  (void)(null == nullptr);
  (void)(null <= nullptr);
  (void)(null == 0);
  (void)(null == (void*)0);
  (void)((void*)0 == nullptr);
  (void)(null <= 0);
  (void)(null <= (void*)0);
  (void)((void*)0 <= nullptr);
  (void)(0 == nullptr);
  (void)(nullptr == 0);
  (void)(nullptr <= 0);
  (void)(0 <= nullptr);
  (void)(1 > nullptr);
  (void)(1 != nullptr);
  (void)(1 + nullptr);
  (void)(0 ? nullptr : 0);
  (void)(0 ? nullptr : (void*)0);
  (void)(0 ? nullptr : A());
  (void)(0 ? A() : nullptr);


  int t = o1(nullptr);
  t = o1(null);
  o2(nullptr);


  (void)&nullptr;
  nullptr_t *pn = &null;


  (void)reinterpret_cast<uintptr_t>(nullptr);
  (void)reinterpret_cast<uintptr_t>(*pn);


  (void)reinterpret_cast<char>(nullptr);

  int *ip = *pn;
  if (*pn) { }


  throw nullptr;
}


template <int *PI, void (*PF)(), int A::*PM, void (A::*PMF)()>
struct T {};

typedef T<nullptr, nullptr, nullptr, nullptr> NT;

namespace test1 {
template<typename T, typename U> struct is_same {
  static const bool value = false;
};

template<typename T> struct is_same<T, T> {
  static const bool value = true;
};

void *g(void*);
bool g(bool);


static_assert(is_same<decltype(g(nullptr)), void*>::value, "");
}

namespace test2 {
  void f(int, ...) __attribute__((sentinel));

  void g() {

    f(10, nullptr);
  }
}

namespace test3 {
  void f(const char*, ...) __attribute__((format(printf, 1, 2)));

  void g() {

    f("%p", nullptr);
  }
}

static_assert(__is_scalar(nullptr_t), "");
static_assert(__is_pod(nullptr_t), "");
static_assert(sizeof(nullptr_t) == sizeof(void*), "");

static_assert(!(nullptr < nullptr), "");
static_assert(!(nullptr > nullptr), "");
static_assert( nullptr <= nullptr, "");
static_assert( nullptr >= nullptr, "");
static_assert( nullptr == nullptr, "");
static_assert(!(nullptr != nullptr), "");

static_assert(!(0 < nullptr), "");
static_assert(!(0 > nullptr), "");
static_assert( 0 <= nullptr, "");
static_assert( 0 >= nullptr, "");
static_assert( 0 == nullptr, "");
static_assert(!(0 != nullptr), "");

static_assert(!(nullptr < 0), "");
static_assert(!(nullptr > 0), "");
static_assert( nullptr <= 0, "");
static_assert( nullptr >= 0, "");
static_assert( nullptr == 0, "");
static_assert(!(nullptr != 0), "");

namespace overloading {
  int &f1(int*);
  float &f1(bool);

  void test_f1() {
    int &ir = (f1)(nullptr);
  }

  struct ConvertsToNullPtr {
    operator nullptr_t() const;
  };

  void test_conversion(ConvertsToNullPtr ctn) {
    (void)(ctn == ctn);
    (void)(ctn != ctn);
    (void)(ctn <= ctn);
    (void)(ctn >= ctn);
    (void)(ctn < ctn);
    (void)(ctn > ctn);
  }
}

namespace templates {
  template<typename T, nullptr_t Value>
  struct X {
    X() { ptr = Value; }

    T *ptr;
  };

  X<int, nullptr> x;


  template<int (*fp)(int), int* p, int A::* pmd, int (A::*pmf)(int)>
  struct X2 {};

  X2<nullptr, nullptr, nullptr, nullptr> x2;
}

namespace null_pointer_constant {



struct S { int n; };
constexpr int null() { return 0; }
void *p = S().n;
void *q = null();

}
