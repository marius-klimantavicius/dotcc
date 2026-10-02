//type: fp
//options: 
# 1 "SemaCXX/warn-cast-qual.cpp"
# 1 "<built-in>" 1
# 1 "<built-in>" 3
# 482 "<built-in>" 3
# 1 "<command line>" 1
# 1 "<built-in>" 2
# 1 "SemaCXX/warn-cast-qual.cpp" 2


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
# 4 "SemaCXX/warn-cast-qual.cpp" 2



void foo_ptr() {
  const char *const ptr = 0;
  char *t0 = const_cast<char *>(ptr);

  volatile char *ptr2 = 0;
  char *t1 = const_cast<char *>(ptr2);

  const volatile char *ptr3 = 0;
  char *t2 = const_cast<char *>(ptr3);
}

void cstr() {
  void* p0 = (void*)(const void*)"txt";
  void* p1 = (void*)"txt";
  char* p2 = (char*)"txt";
}

void foo_0() {
  const int a = 0;

  const int &a0 = a;
  const int &a1 = (const int &)a;

  int &a2 = (int &)a;
  const int &a3 = (int &)a;
  int &a4 = (int &)((const int &)a);
  int &a5 = (int &)((int &)a);
  const int &a6 = (int &)((int &)a);
  const int &a7 = (int &)((const int &)a);
  const int &a8 = (const int &)((int &)a);
}

void foo_1() {
  volatile int a = 0;

  volatile int &a0 = a;
  volatile int &a1 = (volatile int &)a;

  int &a2 = (int &)a;
  volatile int &a3 = (int &)a;
  int &a4 = (int &)((volatile int &)a);
  int &a5 = (int &)((int &)a);
  volatile int &a6 = (int &)((int &)a);
  volatile int &a7 = (int &)((volatile int &)a);
  volatile int &a8 = (volatile int &)((int &)a);
}

void foo_2() {
  const volatile int a = 0;

  const volatile int &a0 = a;
  const volatile int &a1 = (const volatile int &)a;

  int &a2 = (int &)a;
  const volatile int &a3 = (int &)a;
  int &a4 = (int &)((const volatile int &)a);
  int &a5 = (int &)((int &)a);
  const volatile int &a6 = (int &)((int &)a);
  const volatile int &a7 = (int &)((const volatile int &)a);
  const volatile int &a8 = (const volatile int &)((int &)a);
}

void bar_0() {
  const int *_a = 0;
  const int **a = &_a;

  int **a0 = (int **)((const int **)a);
  int **a1 = (int **)((int **)a);




  const int **a4 = (const int **)((int **)a);
  const int **a5 = (const int **)((const int **)a);
}

void bar_1() {
  const int *_a = 0;
  const int *&a = _a;

  int *&a0 = (int *&)((const int *&)a);
  int *&a1 = (int *&)((int *&)a);




  const int *&a4 = (const int *&)((int *&)a);
  const int *&a5 = (const int *&)((const int *&)a);
}

void baz_0() {
  struct C {
    void A() {}
    void B() const {}
  };

  const C S;
  S.B();

  ((C &)S).B();
  ((C &)S).A();

  ((C *)&S)->B();
  ((C *)&S)->A();
}

void baz_1() {
  struct C {
    const int a;
    int b;

    C() : a(0) {}
  };

  {
    C S;
    S.b = 0;

    (int &)(S.a) = 0;
    (int &)(S.b) = 0;

    *(int *)(&S.a) = 0;
    *(int *)(&S.b) = 0;
  }
  {
    const C S;

    (int &)(S.a) = 0;
    (int &)(S.b) = 0;

    *(int *)(&S.a) = 0;
    *(int *)(&S.b) = 0;
  }
}
