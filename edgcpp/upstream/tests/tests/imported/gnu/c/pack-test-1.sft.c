//type: fp
//options: 
# 0 "./pack-test-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pack-test-1.c"
# 10 "./pack-test-1.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 1 3 4
# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4

# 145 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long int ptrdiff_t;
# 214 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef long unsigned int size_t;
# 329 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef int wchar_t;
# 425 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
typedef struct {
  long long __max_align_ll __attribute__((__aligned__(__alignof__(long long))));
  long double __max_align_ld __attribute__((__aligned__(__alignof__(long double))));
# 436 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
} max_align_t;
# 450 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stddef.h" 3 4
  typedef __typeof__(nullptr) nullptr_t;
# 11 "./pack-test-1.c" 2
# 38 "./pack-test-1.c"
# 1 "./pack-test-1.h" 1



# 3 "./pack-test-1.h"
struct s0 {
  char f0;
  double f1;
  short f2;
  double f3;
  int f4;
  double f5;
  double f6;
  double f7;
};
# 39 "./pack-test-1.c" 2



#pragma pack(push, p1, 1)
# 1 "./pack-test-1.h" 1


struct s1 {
  char f0;
  double f1;
  short f2;
  double f3;
  int f4;
  double f5;
  double f6;
  double f7;
};
# 44 "./pack-test-1.c" 2

void s1() {
  int test_0 [(
# 46 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 46 "./pack-test-1.c"
 struct s1
# 46 "./pack-test-1.c" 3 4
 , 
# 46 "./pack-test-1.c"
 f1
# 46 "./pack-test-1.c" 3 4
 ) 
# 46 "./pack-test-1.c"
 == (((((
# 46 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 46 "./pack-test-1.c"
 struct s1
# 46 "./pack-test-1.c" 3 4
 , 
# 46 "./pack-test-1.c"
 f0
# 46 "./pack-test-1.c" 3 4
 ) 
# 46 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 46 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 46 "./pack-test-1.c"
 struct s1
# 46 "./pack-test-1.c" 3 4
 , 
# 46 "./pack-test-1.c"
 f0
# 46 "./pack-test-1.c" 3 4
 ) 
# 46 "./pack-test-1.c"
 + sizeof (char) + (1) - 1)) / (1) * (1))) ? ((((
# 46 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 46 "./pack-test-1.c"
 struct s1
# 46 "./pack-test-1.c" 3 4
 , 
# 46 "./pack-test-1.c"
 f0
# 46 "./pack-test-1.c" 3 4
 ) 
# 46 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 46 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 46 "./pack-test-1.c"
 struct s1
# 46 "./pack-test-1.c" 3 4
 , 
# 46 "./pack-test-1.c"
 f0
# 46 "./pack-test-1.c" 3 4
 ) 
# 46 "./pack-test-1.c"
 + sizeof (char) + (1) - 1)) / (1) * (1))))) ? 1 : -1];
  int test_1 [(
# 47 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 47 "./pack-test-1.c"
 struct s1
# 47 "./pack-test-1.c" 3 4
 , 
# 47 "./pack-test-1.c"
 f3
# 47 "./pack-test-1.c" 3 4
 ) 
# 47 "./pack-test-1.c"
 == (((((
# 47 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 47 "./pack-test-1.c"
 struct s1
# 47 "./pack-test-1.c" 3 4
 , 
# 47 "./pack-test-1.c"
 f2
# 47 "./pack-test-1.c" 3 4
 ) 
# 47 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 47 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 47 "./pack-test-1.c"
 struct s1
# 47 "./pack-test-1.c" 3 4
 , 
# 47 "./pack-test-1.c"
 f2
# 47 "./pack-test-1.c" 3 4
 ) 
# 47 "./pack-test-1.c"
 + sizeof (short) + (1) - 1)) / (1) * (1))) ? ((((
# 47 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 47 "./pack-test-1.c"
 struct s1
# 47 "./pack-test-1.c" 3 4
 , 
# 47 "./pack-test-1.c"
 f2
# 47 "./pack-test-1.c" 3 4
 ) 
# 47 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 47 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 47 "./pack-test-1.c"
 struct s1
# 47 "./pack-test-1.c" 3 4
 , 
# 47 "./pack-test-1.c"
 f2
# 47 "./pack-test-1.c" 3 4
 ) 
# 47 "./pack-test-1.c"
 + sizeof (short) + (1) - 1)) / (1) * (1))))) ? 1 : -1];
  int test_2 [(
# 48 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 48 "./pack-test-1.c"
 struct s1
# 48 "./pack-test-1.c" 3 4
 , 
# 48 "./pack-test-1.c"
 f5
# 48 "./pack-test-1.c" 3 4
 ) 
# 48 "./pack-test-1.c"
 == (((((
# 48 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 48 "./pack-test-1.c"
 struct s1
# 48 "./pack-test-1.c" 3 4
 , 
# 48 "./pack-test-1.c"
 f4
# 48 "./pack-test-1.c" 3 4
 ) 
# 48 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 48 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 48 "./pack-test-1.c"
 struct s1
# 48 "./pack-test-1.c" 3 4
 , 
# 48 "./pack-test-1.c"
 f4
# 48 "./pack-test-1.c" 3 4
 ) 
# 48 "./pack-test-1.c"
 + sizeof (int) + (1) - 1)) / (1) * (1))) ? ((((
# 48 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 48 "./pack-test-1.c"
 struct s1
# 48 "./pack-test-1.c" 3 4
 , 
# 48 "./pack-test-1.c"
 f4
# 48 "./pack-test-1.c" 3 4
 ) 
# 48 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 48 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 48 "./pack-test-1.c"
 struct s1
# 48 "./pack-test-1.c" 3 4
 , 
# 48 "./pack-test-1.c"
 f4
# 48 "./pack-test-1.c" 3 4
 ) 
# 48 "./pack-test-1.c"
 + sizeof (int) + (1) - 1)) / (1) * (1))))) ? 1 : -1];
}



#pragma pack(push, p2, 2)
# 1 "./pack-test-1.h" 1


struct s2 {
  char f0;
  double f1;
  short f2;
  double f3;
  int f4;
  double f5;
  double f6;
  double f7;
};
# 55 "./pack-test-1.c" 2

void s2() {
  int test_0 [(
# 57 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 57 "./pack-test-1.c"
 struct s2
# 57 "./pack-test-1.c" 3 4
 , 
# 57 "./pack-test-1.c"
 f1
# 57 "./pack-test-1.c" 3 4
 ) 
# 57 "./pack-test-1.c"
 == (((((
# 57 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 57 "./pack-test-1.c"
 struct s2
# 57 "./pack-test-1.c" 3 4
 , 
# 57 "./pack-test-1.c"
 f0
# 57 "./pack-test-1.c" 3 4
 ) 
# 57 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 57 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 57 "./pack-test-1.c"
 struct s2
# 57 "./pack-test-1.c" 3 4
 , 
# 57 "./pack-test-1.c"
 f0
# 57 "./pack-test-1.c" 3 4
 ) 
# 57 "./pack-test-1.c"
 + sizeof (char) + (2) - 1)) / (2) * (2))) ? ((((
# 57 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 57 "./pack-test-1.c"
 struct s2
# 57 "./pack-test-1.c" 3 4
 , 
# 57 "./pack-test-1.c"
 f0
# 57 "./pack-test-1.c" 3 4
 ) 
# 57 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 57 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 57 "./pack-test-1.c"
 struct s2
# 57 "./pack-test-1.c" 3 4
 , 
# 57 "./pack-test-1.c"
 f0
# 57 "./pack-test-1.c" 3 4
 ) 
# 57 "./pack-test-1.c"
 + sizeof (char) + (2) - 1)) / (2) * (2))))) ? 1 : -1];
  int test_1 [(
# 58 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 58 "./pack-test-1.c"
 struct s2
# 58 "./pack-test-1.c" 3 4
 , 
# 58 "./pack-test-1.c"
 f3
# 58 "./pack-test-1.c" 3 4
 ) 
# 58 "./pack-test-1.c"
 == (((((
# 58 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 58 "./pack-test-1.c"
 struct s2
# 58 "./pack-test-1.c" 3 4
 , 
# 58 "./pack-test-1.c"
 f2
# 58 "./pack-test-1.c" 3 4
 ) 
# 58 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 58 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 58 "./pack-test-1.c"
 struct s2
# 58 "./pack-test-1.c" 3 4
 , 
# 58 "./pack-test-1.c"
 f2
# 58 "./pack-test-1.c" 3 4
 ) 
# 58 "./pack-test-1.c"
 + sizeof (short) + (2) - 1)) / (2) * (2))) ? ((((
# 58 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 58 "./pack-test-1.c"
 struct s2
# 58 "./pack-test-1.c" 3 4
 , 
# 58 "./pack-test-1.c"
 f2
# 58 "./pack-test-1.c" 3 4
 ) 
# 58 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 58 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 58 "./pack-test-1.c"
 struct s2
# 58 "./pack-test-1.c" 3 4
 , 
# 58 "./pack-test-1.c"
 f2
# 58 "./pack-test-1.c" 3 4
 ) 
# 58 "./pack-test-1.c"
 + sizeof (short) + (2) - 1)) / (2) * (2))))) ? 1 : -1];
  int test_2 [(
# 59 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 59 "./pack-test-1.c"
 struct s2
# 59 "./pack-test-1.c" 3 4
 , 
# 59 "./pack-test-1.c"
 f5
# 59 "./pack-test-1.c" 3 4
 ) 
# 59 "./pack-test-1.c"
 == (((((
# 59 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 59 "./pack-test-1.c"
 struct s2
# 59 "./pack-test-1.c" 3 4
 , 
# 59 "./pack-test-1.c"
 f4
# 59 "./pack-test-1.c" 3 4
 ) 
# 59 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 59 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 59 "./pack-test-1.c"
 struct s2
# 59 "./pack-test-1.c" 3 4
 , 
# 59 "./pack-test-1.c"
 f4
# 59 "./pack-test-1.c" 3 4
 ) 
# 59 "./pack-test-1.c"
 + sizeof (int) + (2) - 1)) / (2) * (2))) ? ((((
# 59 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 59 "./pack-test-1.c"
 struct s2
# 59 "./pack-test-1.c" 3 4
 , 
# 59 "./pack-test-1.c"
 f4
# 59 "./pack-test-1.c" 3 4
 ) 
# 59 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 59 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 59 "./pack-test-1.c"
 struct s2
# 59 "./pack-test-1.c" 3 4
 , 
# 59 "./pack-test-1.c"
 f4
# 59 "./pack-test-1.c" 3 4
 ) 
# 59 "./pack-test-1.c"
 + sizeof (int) + (2) - 1)) / (2) * (2))))) ? 1 : -1];
}



#pragma pack(push, p3, 4)
# 1 "./pack-test-1.h" 1


struct s3 {
  char f0;
  double f1;
  short f2;
  double f3;
  int f4;
  double f5;
  double f6;
  double f7;
};
# 66 "./pack-test-1.c" 2

void s3() {
  int test_0 [(
# 68 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 68 "./pack-test-1.c"
 struct s3
# 68 "./pack-test-1.c" 3 4
 , 
# 68 "./pack-test-1.c"
 f1
# 68 "./pack-test-1.c" 3 4
 ) 
# 68 "./pack-test-1.c"
 == (((((
# 68 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 68 "./pack-test-1.c"
 struct s3
# 68 "./pack-test-1.c" 3 4
 , 
# 68 "./pack-test-1.c"
 f0
# 68 "./pack-test-1.c" 3 4
 ) 
# 68 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 68 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 68 "./pack-test-1.c"
 struct s3
# 68 "./pack-test-1.c" 3 4
 , 
# 68 "./pack-test-1.c"
 f0
# 68 "./pack-test-1.c" 3 4
 ) 
# 68 "./pack-test-1.c"
 + sizeof (char) + (4) - 1)) / (4) * (4))) ? ((((
# 68 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 68 "./pack-test-1.c"
 struct s3
# 68 "./pack-test-1.c" 3 4
 , 
# 68 "./pack-test-1.c"
 f0
# 68 "./pack-test-1.c" 3 4
 ) 
# 68 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 68 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 68 "./pack-test-1.c"
 struct s3
# 68 "./pack-test-1.c" 3 4
 , 
# 68 "./pack-test-1.c"
 f0
# 68 "./pack-test-1.c" 3 4
 ) 
# 68 "./pack-test-1.c"
 + sizeof (char) + (4) - 1)) / (4) * (4))))) ? 1 : -1];
  int test_1 [(
# 69 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 69 "./pack-test-1.c"
 struct s3
# 69 "./pack-test-1.c" 3 4
 , 
# 69 "./pack-test-1.c"
 f3
# 69 "./pack-test-1.c" 3 4
 ) 
# 69 "./pack-test-1.c"
 == (((((
# 69 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 69 "./pack-test-1.c"
 struct s3
# 69 "./pack-test-1.c" 3 4
 , 
# 69 "./pack-test-1.c"
 f2
# 69 "./pack-test-1.c" 3 4
 ) 
# 69 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 69 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 69 "./pack-test-1.c"
 struct s3
# 69 "./pack-test-1.c" 3 4
 , 
# 69 "./pack-test-1.c"
 f2
# 69 "./pack-test-1.c" 3 4
 ) 
# 69 "./pack-test-1.c"
 + sizeof (short) + (4) - 1)) / (4) * (4))) ? ((((
# 69 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 69 "./pack-test-1.c"
 struct s3
# 69 "./pack-test-1.c" 3 4
 , 
# 69 "./pack-test-1.c"
 f2
# 69 "./pack-test-1.c" 3 4
 ) 
# 69 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 69 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 69 "./pack-test-1.c"
 struct s3
# 69 "./pack-test-1.c" 3 4
 , 
# 69 "./pack-test-1.c"
 f2
# 69 "./pack-test-1.c" 3 4
 ) 
# 69 "./pack-test-1.c"
 + sizeof (short) + (4) - 1)) / (4) * (4))))) ? 1 : -1];
  int test_2 [(
# 70 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 70 "./pack-test-1.c"
 struct s3
# 70 "./pack-test-1.c" 3 4
 , 
# 70 "./pack-test-1.c"
 f5
# 70 "./pack-test-1.c" 3 4
 ) 
# 70 "./pack-test-1.c"
 == (((((
# 70 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 70 "./pack-test-1.c"
 struct s3
# 70 "./pack-test-1.c" 3 4
 , 
# 70 "./pack-test-1.c"
 f4
# 70 "./pack-test-1.c" 3 4
 ) 
# 70 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 70 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 70 "./pack-test-1.c"
 struct s3
# 70 "./pack-test-1.c" 3 4
 , 
# 70 "./pack-test-1.c"
 f4
# 70 "./pack-test-1.c" 3 4
 ) 
# 70 "./pack-test-1.c"
 + sizeof (int) + (4) - 1)) / (4) * (4))) ? ((((
# 70 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 70 "./pack-test-1.c"
 struct s3
# 70 "./pack-test-1.c" 3 4
 , 
# 70 "./pack-test-1.c"
 f4
# 70 "./pack-test-1.c" 3 4
 ) 
# 70 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 70 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 70 "./pack-test-1.c"
 struct s3
# 70 "./pack-test-1.c" 3 4
 , 
# 70 "./pack-test-1.c"
 f4
# 70 "./pack-test-1.c" 3 4
 ) 
# 70 "./pack-test-1.c"
 + sizeof (int) + (4) - 1)) / (4) * (4))))) ? 1 : -1];
}



#pragma pack(pop)
# 1 "./pack-test-1.h" 1


struct s4 {
  char f0;
  double f1;
  short f2;
  double f3;
  int f4;
  double f5;
  double f6;
  double f7;
};
# 77 "./pack-test-1.c" 2

void s4() {
  int test_0 [(
# 79 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 79 "./pack-test-1.c"
 struct s4
# 79 "./pack-test-1.c" 3 4
 , 
# 79 "./pack-test-1.c"
 f1
# 79 "./pack-test-1.c" 3 4
 ) 
# 79 "./pack-test-1.c"
 == (((((
# 79 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 79 "./pack-test-1.c"
 struct s4
# 79 "./pack-test-1.c" 3 4
 , 
# 79 "./pack-test-1.c"
 f0
# 79 "./pack-test-1.c" 3 4
 ) 
# 79 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 79 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 79 "./pack-test-1.c"
 struct s4
# 79 "./pack-test-1.c" 3 4
 , 
# 79 "./pack-test-1.c"
 f0
# 79 "./pack-test-1.c" 3 4
 ) 
# 79 "./pack-test-1.c"
 + sizeof (char) + (2) - 1)) / (2) * (2))) ? ((((
# 79 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 79 "./pack-test-1.c"
 struct s4
# 79 "./pack-test-1.c" 3 4
 , 
# 79 "./pack-test-1.c"
 f0
# 79 "./pack-test-1.c" 3 4
 ) 
# 79 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 79 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 79 "./pack-test-1.c"
 struct s4
# 79 "./pack-test-1.c" 3 4
 , 
# 79 "./pack-test-1.c"
 f0
# 79 "./pack-test-1.c" 3 4
 ) 
# 79 "./pack-test-1.c"
 + sizeof (char) + (2) - 1)) / (2) * (2))))) ? 1 : -1];
  int test_1 [(
# 80 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 80 "./pack-test-1.c"
 struct s4
# 80 "./pack-test-1.c" 3 4
 , 
# 80 "./pack-test-1.c"
 f3
# 80 "./pack-test-1.c" 3 4
 ) 
# 80 "./pack-test-1.c"
 == (((((
# 80 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 80 "./pack-test-1.c"
 struct s4
# 80 "./pack-test-1.c" 3 4
 , 
# 80 "./pack-test-1.c"
 f2
# 80 "./pack-test-1.c" 3 4
 ) 
# 80 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 80 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 80 "./pack-test-1.c"
 struct s4
# 80 "./pack-test-1.c" 3 4
 , 
# 80 "./pack-test-1.c"
 f2
# 80 "./pack-test-1.c" 3 4
 ) 
# 80 "./pack-test-1.c"
 + sizeof (short) + (2) - 1)) / (2) * (2))) ? ((((
# 80 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 80 "./pack-test-1.c"
 struct s4
# 80 "./pack-test-1.c" 3 4
 , 
# 80 "./pack-test-1.c"
 f2
# 80 "./pack-test-1.c" 3 4
 ) 
# 80 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 80 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 80 "./pack-test-1.c"
 struct s4
# 80 "./pack-test-1.c" 3 4
 , 
# 80 "./pack-test-1.c"
 f2
# 80 "./pack-test-1.c" 3 4
 ) 
# 80 "./pack-test-1.c"
 + sizeof (short) + (2) - 1)) / (2) * (2))))) ? 1 : -1];
  int test_2 [(
# 81 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 81 "./pack-test-1.c"
 struct s4
# 81 "./pack-test-1.c" 3 4
 , 
# 81 "./pack-test-1.c"
 f5
# 81 "./pack-test-1.c" 3 4
 ) 
# 81 "./pack-test-1.c"
 == (((((
# 81 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 81 "./pack-test-1.c"
 struct s4
# 81 "./pack-test-1.c" 3 4
 , 
# 81 "./pack-test-1.c"
 f4
# 81 "./pack-test-1.c" 3 4
 ) 
# 81 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 81 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 81 "./pack-test-1.c"
 struct s4
# 81 "./pack-test-1.c" 3 4
 , 
# 81 "./pack-test-1.c"
 f4
# 81 "./pack-test-1.c" 3 4
 ) 
# 81 "./pack-test-1.c"
 + sizeof (int) + (2) - 1)) / (2) * (2))) ? ((((
# 81 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 81 "./pack-test-1.c"
 struct s4
# 81 "./pack-test-1.c" 3 4
 , 
# 81 "./pack-test-1.c"
 f4
# 81 "./pack-test-1.c" 3 4
 ) 
# 81 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 81 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 81 "./pack-test-1.c"
 struct s4
# 81 "./pack-test-1.c" 3 4
 , 
# 81 "./pack-test-1.c"
 f4
# 81 "./pack-test-1.c" 3 4
 ) 
# 81 "./pack-test-1.c"
 + sizeof (int) + (2) - 1)) / (2) * (2))))) ? 1 : -1];
}



#pragma pack(pop, p2)
# 1 "./pack-test-1.h" 1


struct s5 {
  char f0;
  double f1;
  short f2;
  double f3;
  int f4;
  double f5;
  double f6;
  double f7;
};
# 88 "./pack-test-1.c" 2

void s5() {
  int test_0 [(
# 90 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 90 "./pack-test-1.c"
 struct s5
# 90 "./pack-test-1.c" 3 4
 , 
# 90 "./pack-test-1.c"
 f1
# 90 "./pack-test-1.c" 3 4
 ) 
# 90 "./pack-test-1.c"
 == (((((
# 90 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 90 "./pack-test-1.c"
 struct s5
# 90 "./pack-test-1.c" 3 4
 , 
# 90 "./pack-test-1.c"
 f0
# 90 "./pack-test-1.c" 3 4
 ) 
# 90 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 90 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 90 "./pack-test-1.c"
 struct s5
# 90 "./pack-test-1.c" 3 4
 , 
# 90 "./pack-test-1.c"
 f0
# 90 "./pack-test-1.c" 3 4
 ) 
# 90 "./pack-test-1.c"
 + sizeof (char) + (1) - 1)) / (1) * (1))) ? ((((
# 90 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 90 "./pack-test-1.c"
 struct s5
# 90 "./pack-test-1.c" 3 4
 , 
# 90 "./pack-test-1.c"
 f0
# 90 "./pack-test-1.c" 3 4
 ) 
# 90 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 90 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 90 "./pack-test-1.c"
 struct s5
# 90 "./pack-test-1.c" 3 4
 , 
# 90 "./pack-test-1.c"
 f0
# 90 "./pack-test-1.c" 3 4
 ) 
# 90 "./pack-test-1.c"
 + sizeof (char) + (1) - 1)) / (1) * (1))))) ? 1 : -1];
  int test_1 [(
# 91 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 91 "./pack-test-1.c"
 struct s5
# 91 "./pack-test-1.c" 3 4
 , 
# 91 "./pack-test-1.c"
 f3
# 91 "./pack-test-1.c" 3 4
 ) 
# 91 "./pack-test-1.c"
 == (((((
# 91 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 91 "./pack-test-1.c"
 struct s5
# 91 "./pack-test-1.c" 3 4
 , 
# 91 "./pack-test-1.c"
 f2
# 91 "./pack-test-1.c" 3 4
 ) 
# 91 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 91 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 91 "./pack-test-1.c"
 struct s5
# 91 "./pack-test-1.c" 3 4
 , 
# 91 "./pack-test-1.c"
 f2
# 91 "./pack-test-1.c" 3 4
 ) 
# 91 "./pack-test-1.c"
 + sizeof (short) + (1) - 1)) / (1) * (1))) ? ((((
# 91 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 91 "./pack-test-1.c"
 struct s5
# 91 "./pack-test-1.c" 3 4
 , 
# 91 "./pack-test-1.c"
 f2
# 91 "./pack-test-1.c" 3 4
 ) 
# 91 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 91 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 91 "./pack-test-1.c"
 struct s5
# 91 "./pack-test-1.c" 3 4
 , 
# 91 "./pack-test-1.c"
 f2
# 91 "./pack-test-1.c" 3 4
 ) 
# 91 "./pack-test-1.c"
 + sizeof (short) + (1) - 1)) / (1) * (1))))) ? 1 : -1];
  int test_2 [(
# 92 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 92 "./pack-test-1.c"
 struct s5
# 92 "./pack-test-1.c" 3 4
 , 
# 92 "./pack-test-1.c"
 f5
# 92 "./pack-test-1.c" 3 4
 ) 
# 92 "./pack-test-1.c"
 == (((((
# 92 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 92 "./pack-test-1.c"
 struct s5
# 92 "./pack-test-1.c" 3 4
 , 
# 92 "./pack-test-1.c"
 f4
# 92 "./pack-test-1.c" 3 4
 ) 
# 92 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 92 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 92 "./pack-test-1.c"
 struct s5
# 92 "./pack-test-1.c" 3 4
 , 
# 92 "./pack-test-1.c"
 f4
# 92 "./pack-test-1.c" 3 4
 ) 
# 92 "./pack-test-1.c"
 + sizeof (int) + (1) - 1)) / (1) * (1))) ? ((((
# 92 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 92 "./pack-test-1.c"
 struct s5
# 92 "./pack-test-1.c" 3 4
 , 
# 92 "./pack-test-1.c"
 f4
# 92 "./pack-test-1.c" 3 4
 ) 
# 92 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 92 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 92 "./pack-test-1.c"
 struct s5
# 92 "./pack-test-1.c" 3 4
 , 
# 92 "./pack-test-1.c"
 f4
# 92 "./pack-test-1.c" 3 4
 ) 
# 92 "./pack-test-1.c"
 + sizeof (int) + (1) - 1)) / (1) * (1))))) ? 1 : -1];
}



#pragma pack(pop, p1)
# 1 "./pack-test-1.h" 1


struct s6 {
  char f0;
  double f1;
  short f2;
  double f3;
  int f4;
  double f5;
  double f6;
  double f7;
};
# 99 "./pack-test-1.c" 2

void s6() {
  int test_0 [((
# 101 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 101 "./pack-test-1.c"
 struct s0
# 101 "./pack-test-1.c" 3 4
 , 
# 101 "./pack-test-1.c"
 f0
# 101 "./pack-test-1.c" 3 4
 ) 
# 101 "./pack-test-1.c"
 == 
# 101 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 101 "./pack-test-1.c"
 struct s6
# 101 "./pack-test-1.c" 3 4
 , 
# 101 "./pack-test-1.c"
 f0
# 101 "./pack-test-1.c" 3 4
 )
# 101 "./pack-test-1.c"
 )) ? 1 : -1];
  int test_1 [((
# 102 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 102 "./pack-test-1.c"
 struct s0
# 102 "./pack-test-1.c" 3 4
 , 
# 102 "./pack-test-1.c"
 f1
# 102 "./pack-test-1.c" 3 4
 ) 
# 102 "./pack-test-1.c"
 == 
# 102 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 102 "./pack-test-1.c"
 struct s6
# 102 "./pack-test-1.c" 3 4
 , 
# 102 "./pack-test-1.c"
 f1
# 102 "./pack-test-1.c" 3 4
 )
# 102 "./pack-test-1.c"
 )) ? 1 : -1];
  int test_2 [((
# 103 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 103 "./pack-test-1.c"
 struct s0
# 103 "./pack-test-1.c" 3 4
 , 
# 103 "./pack-test-1.c"
 f2
# 103 "./pack-test-1.c" 3 4
 ) 
# 103 "./pack-test-1.c"
 == 
# 103 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 103 "./pack-test-1.c"
 struct s6
# 103 "./pack-test-1.c" 3 4
 , 
# 103 "./pack-test-1.c"
 f2
# 103 "./pack-test-1.c" 3 4
 )
# 103 "./pack-test-1.c"
 )) ? 1 : -1];
  int test_3 [((
# 104 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 104 "./pack-test-1.c"
 struct s0
# 104 "./pack-test-1.c" 3 4
 , 
# 104 "./pack-test-1.c"
 f3
# 104 "./pack-test-1.c" 3 4
 ) 
# 104 "./pack-test-1.c"
 == 
# 104 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 104 "./pack-test-1.c"
 struct s6
# 104 "./pack-test-1.c" 3 4
 , 
# 104 "./pack-test-1.c"
 f3
# 104 "./pack-test-1.c" 3 4
 )
# 104 "./pack-test-1.c"
 )) ? 1 : -1];
  int test_4 [((
# 105 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 105 "./pack-test-1.c"
 struct s0
# 105 "./pack-test-1.c" 3 4
 , 
# 105 "./pack-test-1.c"
 f4
# 105 "./pack-test-1.c" 3 4
 ) 
# 105 "./pack-test-1.c"
 == 
# 105 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 105 "./pack-test-1.c"
 struct s6
# 105 "./pack-test-1.c" 3 4
 , 
# 105 "./pack-test-1.c"
 f4
# 105 "./pack-test-1.c" 3 4
 )
# 105 "./pack-test-1.c"
 )) ? 1 : -1];
  int test_5 [((
# 106 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 106 "./pack-test-1.c"
 struct s0
# 106 "./pack-test-1.c" 3 4
 , 
# 106 "./pack-test-1.c"
 f5
# 106 "./pack-test-1.c" 3 4
 ) 
# 106 "./pack-test-1.c"
 == 
# 106 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 106 "./pack-test-1.c"
 struct s6
# 106 "./pack-test-1.c" 3 4
 , 
# 106 "./pack-test-1.c"
 f5
# 106 "./pack-test-1.c" 3 4
 )
# 106 "./pack-test-1.c"
 )) ? 1 : -1];
}



#pragma pack(1)
# 1 "./pack-test-1.h" 1


struct s7 {
  char f0;
  double f1;
  short f2;
  double f3;
  int f4;
  double f5;
  double f6;
  double f7;
};
# 113 "./pack-test-1.c" 2

void s7() {
  int test_0 [(
# 115 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 115 "./pack-test-1.c"
 struct s7
# 115 "./pack-test-1.c" 3 4
 , 
# 115 "./pack-test-1.c"
 f1
# 115 "./pack-test-1.c" 3 4
 ) 
# 115 "./pack-test-1.c"
 == (((((
# 115 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 115 "./pack-test-1.c"
 struct s7
# 115 "./pack-test-1.c" 3 4
 , 
# 115 "./pack-test-1.c"
 f0
# 115 "./pack-test-1.c" 3 4
 ) 
# 115 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 115 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 115 "./pack-test-1.c"
 struct s7
# 115 "./pack-test-1.c" 3 4
 , 
# 115 "./pack-test-1.c"
 f0
# 115 "./pack-test-1.c" 3 4
 ) 
# 115 "./pack-test-1.c"
 + sizeof (char) + (1) - 1)) / (1) * (1))) ? ((((
# 115 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 115 "./pack-test-1.c"
 struct s7
# 115 "./pack-test-1.c" 3 4
 , 
# 115 "./pack-test-1.c"
 f0
# 115 "./pack-test-1.c" 3 4
 ) 
# 115 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 115 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 115 "./pack-test-1.c"
 struct s7
# 115 "./pack-test-1.c" 3 4
 , 
# 115 "./pack-test-1.c"
 f0
# 115 "./pack-test-1.c" 3 4
 ) 
# 115 "./pack-test-1.c"
 + sizeof (char) + (1) - 1)) / (1) * (1))))) ? 1 : -1];
  int test_1 [(
# 116 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 116 "./pack-test-1.c"
 struct s7
# 116 "./pack-test-1.c" 3 4
 , 
# 116 "./pack-test-1.c"
 f3
# 116 "./pack-test-1.c" 3 4
 ) 
# 116 "./pack-test-1.c"
 == (((((
# 116 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 116 "./pack-test-1.c"
 struct s7
# 116 "./pack-test-1.c" 3 4
 , 
# 116 "./pack-test-1.c"
 f2
# 116 "./pack-test-1.c" 3 4
 ) 
# 116 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 116 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 116 "./pack-test-1.c"
 struct s7
# 116 "./pack-test-1.c" 3 4
 , 
# 116 "./pack-test-1.c"
 f2
# 116 "./pack-test-1.c" 3 4
 ) 
# 116 "./pack-test-1.c"
 + sizeof (short) + (1) - 1)) / (1) * (1))) ? ((((
# 116 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 116 "./pack-test-1.c"
 struct s7
# 116 "./pack-test-1.c" 3 4
 , 
# 116 "./pack-test-1.c"
 f2
# 116 "./pack-test-1.c" 3 4
 ) 
# 116 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 116 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 116 "./pack-test-1.c"
 struct s7
# 116 "./pack-test-1.c" 3 4
 , 
# 116 "./pack-test-1.c"
 f2
# 116 "./pack-test-1.c" 3 4
 ) 
# 116 "./pack-test-1.c"
 + sizeof (short) + (1) - 1)) / (1) * (1))))) ? 1 : -1];
  int test_2 [(
# 117 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 117 "./pack-test-1.c"
 struct s7
# 117 "./pack-test-1.c" 3 4
 , 
# 117 "./pack-test-1.c"
 f5
# 117 "./pack-test-1.c" 3 4
 ) 
# 117 "./pack-test-1.c"
 == (((((
# 117 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 117 "./pack-test-1.c"
 struct s7
# 117 "./pack-test-1.c" 3 4
 , 
# 117 "./pack-test-1.c"
 f4
# 117 "./pack-test-1.c" 3 4
 ) 
# 117 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 117 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 117 "./pack-test-1.c"
 struct s7
# 117 "./pack-test-1.c" 3 4
 , 
# 117 "./pack-test-1.c"
 f4
# 117 "./pack-test-1.c" 3 4
 ) 
# 117 "./pack-test-1.c"
 + sizeof (int) + (1) - 1)) / (1) * (1))) ? ((((
# 117 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 117 "./pack-test-1.c"
 struct s7
# 117 "./pack-test-1.c" 3 4
 , 
# 117 "./pack-test-1.c"
 f4
# 117 "./pack-test-1.c" 3 4
 ) 
# 117 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 117 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 117 "./pack-test-1.c"
 struct s7
# 117 "./pack-test-1.c" 3 4
 , 
# 117 "./pack-test-1.c"
 f4
# 117 "./pack-test-1.c" 3 4
 ) 
# 117 "./pack-test-1.c"
 + sizeof (int) + (1) - 1)) / (1) * (1))))) ? 1 : -1];
}



#pragma pack(push, p2, 2)
# 1 "./pack-test-1.h" 1


struct s8 {
  char f0;
  double f1;
  short f2;
  double f3;
  int f4;
  double f5;
  double f6;
  double f7;
};
# 124 "./pack-test-1.c" 2

void s8() {
  int test_0 [(
# 126 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 126 "./pack-test-1.c"
 struct s8
# 126 "./pack-test-1.c" 3 4
 , 
# 126 "./pack-test-1.c"
 f1
# 126 "./pack-test-1.c" 3 4
 ) 
# 126 "./pack-test-1.c"
 == (((((
# 126 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 126 "./pack-test-1.c"
 struct s8
# 126 "./pack-test-1.c" 3 4
 , 
# 126 "./pack-test-1.c"
 f0
# 126 "./pack-test-1.c" 3 4
 ) 
# 126 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 126 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 126 "./pack-test-1.c"
 struct s8
# 126 "./pack-test-1.c" 3 4
 , 
# 126 "./pack-test-1.c"
 f0
# 126 "./pack-test-1.c" 3 4
 ) 
# 126 "./pack-test-1.c"
 + sizeof (char) + (2) - 1)) / (2) * (2))) ? ((((
# 126 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 126 "./pack-test-1.c"
 struct s8
# 126 "./pack-test-1.c" 3 4
 , 
# 126 "./pack-test-1.c"
 f0
# 126 "./pack-test-1.c" 3 4
 ) 
# 126 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 126 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 126 "./pack-test-1.c"
 struct s8
# 126 "./pack-test-1.c" 3 4
 , 
# 126 "./pack-test-1.c"
 f0
# 126 "./pack-test-1.c" 3 4
 ) 
# 126 "./pack-test-1.c"
 + sizeof (char) + (2) - 1)) / (2) * (2))))) ? 1 : -1];
  int test_1 [(
# 127 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 127 "./pack-test-1.c"
 struct s8
# 127 "./pack-test-1.c" 3 4
 , 
# 127 "./pack-test-1.c"
 f3
# 127 "./pack-test-1.c" 3 4
 ) 
# 127 "./pack-test-1.c"
 == (((((
# 127 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 127 "./pack-test-1.c"
 struct s8
# 127 "./pack-test-1.c" 3 4
 , 
# 127 "./pack-test-1.c"
 f2
# 127 "./pack-test-1.c" 3 4
 ) 
# 127 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 127 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 127 "./pack-test-1.c"
 struct s8
# 127 "./pack-test-1.c" 3 4
 , 
# 127 "./pack-test-1.c"
 f2
# 127 "./pack-test-1.c" 3 4
 ) 
# 127 "./pack-test-1.c"
 + sizeof (short) + (2) - 1)) / (2) * (2))) ? ((((
# 127 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 127 "./pack-test-1.c"
 struct s8
# 127 "./pack-test-1.c" 3 4
 , 
# 127 "./pack-test-1.c"
 f2
# 127 "./pack-test-1.c" 3 4
 ) 
# 127 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 127 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 127 "./pack-test-1.c"
 struct s8
# 127 "./pack-test-1.c" 3 4
 , 
# 127 "./pack-test-1.c"
 f2
# 127 "./pack-test-1.c" 3 4
 ) 
# 127 "./pack-test-1.c"
 + sizeof (short) + (2) - 1)) / (2) * (2))))) ? 1 : -1];
  int test_2 [(
# 128 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 128 "./pack-test-1.c"
 struct s8
# 128 "./pack-test-1.c" 3 4
 , 
# 128 "./pack-test-1.c"
 f5
# 128 "./pack-test-1.c" 3 4
 ) 
# 128 "./pack-test-1.c"
 == (((((
# 128 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 128 "./pack-test-1.c"
 struct s8
# 128 "./pack-test-1.c" 3 4
 , 
# 128 "./pack-test-1.c"
 f4
# 128 "./pack-test-1.c" 3 4
 ) 
# 128 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 128 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 128 "./pack-test-1.c"
 struct s8
# 128 "./pack-test-1.c" 3 4
 , 
# 128 "./pack-test-1.c"
 f4
# 128 "./pack-test-1.c" 3 4
 ) 
# 128 "./pack-test-1.c"
 + sizeof (int) + (2) - 1)) / (2) * (2))) ? ((((
# 128 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 128 "./pack-test-1.c"
 struct s8
# 128 "./pack-test-1.c" 3 4
 , 
# 128 "./pack-test-1.c"
 f4
# 128 "./pack-test-1.c" 3 4
 ) 
# 128 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 128 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 128 "./pack-test-1.c"
 struct s8
# 128 "./pack-test-1.c" 3 4
 , 
# 128 "./pack-test-1.c"
 f4
# 128 "./pack-test-1.c" 3 4
 ) 
# 128 "./pack-test-1.c"
 + sizeof (int) + (2) - 1)) / (2) * (2))))) ? 1 : -1];
}



#pragma pack(pop)
# 1 "./pack-test-1.h" 1


struct s9 {
  char f0;
  double f1;
  short f2;
  double f3;
  int f4;
  double f5;
  double f6;
  double f7;
};
# 135 "./pack-test-1.c" 2

void s9() {
  int test_0 [(
# 137 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 137 "./pack-test-1.c"
 struct s9
# 137 "./pack-test-1.c" 3 4
 , 
# 137 "./pack-test-1.c"
 f1
# 137 "./pack-test-1.c" 3 4
 ) 
# 137 "./pack-test-1.c"
 == (((((
# 137 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 137 "./pack-test-1.c"
 struct s9
# 137 "./pack-test-1.c" 3 4
 , 
# 137 "./pack-test-1.c"
 f0
# 137 "./pack-test-1.c" 3 4
 ) 
# 137 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 137 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 137 "./pack-test-1.c"
 struct s9
# 137 "./pack-test-1.c" 3 4
 , 
# 137 "./pack-test-1.c"
 f0
# 137 "./pack-test-1.c" 3 4
 ) 
# 137 "./pack-test-1.c"
 + sizeof (char) + (1) - 1)) / (1) * (1))) ? ((((
# 137 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 137 "./pack-test-1.c"
 struct s9
# 137 "./pack-test-1.c" 3 4
 , 
# 137 "./pack-test-1.c"
 f0
# 137 "./pack-test-1.c" 3 4
 ) 
# 137 "./pack-test-1.c"
 + sizeof (char) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 137 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 137 "./pack-test-1.c"
 struct s9
# 137 "./pack-test-1.c" 3 4
 , 
# 137 "./pack-test-1.c"
 f0
# 137 "./pack-test-1.c" 3 4
 ) 
# 137 "./pack-test-1.c"
 + sizeof (char) + (1) - 1)) / (1) * (1))))) ? 1 : -1];
  int test_1 [(
# 138 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 138 "./pack-test-1.c"
 struct s9
# 138 "./pack-test-1.c" 3 4
 , 
# 138 "./pack-test-1.c"
 f3
# 138 "./pack-test-1.c" 3 4
 ) 
# 138 "./pack-test-1.c"
 == (((((
# 138 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 138 "./pack-test-1.c"
 struct s9
# 138 "./pack-test-1.c" 3 4
 , 
# 138 "./pack-test-1.c"
 f2
# 138 "./pack-test-1.c" 3 4
 ) 
# 138 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 138 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 138 "./pack-test-1.c"
 struct s9
# 138 "./pack-test-1.c" 3 4
 , 
# 138 "./pack-test-1.c"
 f2
# 138 "./pack-test-1.c" 3 4
 ) 
# 138 "./pack-test-1.c"
 + sizeof (short) + (1) - 1)) / (1) * (1))) ? ((((
# 138 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 138 "./pack-test-1.c"
 struct s9
# 138 "./pack-test-1.c" 3 4
 , 
# 138 "./pack-test-1.c"
 f2
# 138 "./pack-test-1.c" 3 4
 ) 
# 138 "./pack-test-1.c"
 + sizeof (short) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 138 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 138 "./pack-test-1.c"
 struct s9
# 138 "./pack-test-1.c" 3 4
 , 
# 138 "./pack-test-1.c"
 f2
# 138 "./pack-test-1.c" 3 4
 ) 
# 138 "./pack-test-1.c"
 + sizeof (short) + (1) - 1)) / (1) * (1))))) ? 1 : -1];
  int test_2 [(
# 139 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 139 "./pack-test-1.c"
 struct s9
# 139 "./pack-test-1.c" 3 4
 , 
# 139 "./pack-test-1.c"
 f5
# 139 "./pack-test-1.c" 3 4
 ) 
# 139 "./pack-test-1.c"
 == (((((
# 139 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 139 "./pack-test-1.c"
 struct s9
# 139 "./pack-test-1.c" 3 4
 , 
# 139 "./pack-test-1.c"
 f4
# 139 "./pack-test-1.c" 3 4
 ) 
# 139 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) < ((((
# 139 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 139 "./pack-test-1.c"
 struct s9
# 139 "./pack-test-1.c" 3 4
 , 
# 139 "./pack-test-1.c"
 f4
# 139 "./pack-test-1.c" 3 4
 ) 
# 139 "./pack-test-1.c"
 + sizeof (int) + (1) - 1)) / (1) * (1))) ? ((((
# 139 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 139 "./pack-test-1.c"
 struct s9
# 139 "./pack-test-1.c" 3 4
 , 
# 139 "./pack-test-1.c"
 f4
# 139 "./pack-test-1.c" 3 4
 ) 
# 139 "./pack-test-1.c"
 + sizeof (int) + (__alignof__(double)) - 1)) / (__alignof__(double)) * (__alignof__(double)))) : ((((
# 139 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 139 "./pack-test-1.c"
 struct s9
# 139 "./pack-test-1.c" 3 4
 , 
# 139 "./pack-test-1.c"
 f4
# 139 "./pack-test-1.c" 3 4
 ) 
# 139 "./pack-test-1.c"
 + sizeof (int) + (1) - 1)) / (1) * (1))))) ? 1 : -1];
}



#pragma pack()
# 1 "./pack-test-1.h" 1


struct s10 {
  char f0;
  double f1;
  short f2;
  double f3;
  int f4;
  double f5;
  double f6;
  double f7;
};
# 146 "./pack-test-1.c" 2

void s10() {
  int test_0 [((
# 148 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 148 "./pack-test-1.c"
 struct s0
# 148 "./pack-test-1.c" 3 4
 , 
# 148 "./pack-test-1.c"
 f0
# 148 "./pack-test-1.c" 3 4
 ) 
# 148 "./pack-test-1.c"
 == 
# 148 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 148 "./pack-test-1.c"
 struct s10
# 148 "./pack-test-1.c" 3 4
 , 
# 148 "./pack-test-1.c"
 f0
# 148 "./pack-test-1.c" 3 4
 )
# 148 "./pack-test-1.c"
 )) ? 1 : -1];
  int test_1 [((
# 149 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 149 "./pack-test-1.c"
 struct s0
# 149 "./pack-test-1.c" 3 4
 , 
# 149 "./pack-test-1.c"
 f1
# 149 "./pack-test-1.c" 3 4
 ) 
# 149 "./pack-test-1.c"
 == 
# 149 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 149 "./pack-test-1.c"
 struct s10
# 149 "./pack-test-1.c" 3 4
 , 
# 149 "./pack-test-1.c"
 f1
# 149 "./pack-test-1.c" 3 4
 )
# 149 "./pack-test-1.c"
 )) ? 1 : -1];
  int test_2 [((
# 150 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 150 "./pack-test-1.c"
 struct s0
# 150 "./pack-test-1.c" 3 4
 , 
# 150 "./pack-test-1.c"
 f2
# 150 "./pack-test-1.c" 3 4
 ) 
# 150 "./pack-test-1.c"
 == 
# 150 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 150 "./pack-test-1.c"
 struct s10
# 150 "./pack-test-1.c" 3 4
 , 
# 150 "./pack-test-1.c"
 f2
# 150 "./pack-test-1.c" 3 4
 )
# 150 "./pack-test-1.c"
 )) ? 1 : -1];
  int test_3 [((
# 151 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 151 "./pack-test-1.c"
 struct s0
# 151 "./pack-test-1.c" 3 4
 , 
# 151 "./pack-test-1.c"
 f3
# 151 "./pack-test-1.c" 3 4
 ) 
# 151 "./pack-test-1.c"
 == 
# 151 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 151 "./pack-test-1.c"
 struct s10
# 151 "./pack-test-1.c" 3 4
 , 
# 151 "./pack-test-1.c"
 f3
# 151 "./pack-test-1.c" 3 4
 )
# 151 "./pack-test-1.c"
 )) ? 1 : -1];
  int test_4 [((
# 152 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 152 "./pack-test-1.c"
 struct s0
# 152 "./pack-test-1.c" 3 4
 , 
# 152 "./pack-test-1.c"
 f4
# 152 "./pack-test-1.c" 3 4
 ) 
# 152 "./pack-test-1.c"
 == 
# 152 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 152 "./pack-test-1.c"
 struct s10
# 152 "./pack-test-1.c" 3 4
 , 
# 152 "./pack-test-1.c"
 f4
# 152 "./pack-test-1.c" 3 4
 )
# 152 "./pack-test-1.c"
 )) ? 1 : -1];
  int test_5 [((
# 153 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 153 "./pack-test-1.c"
 struct s0
# 153 "./pack-test-1.c" 3 4
 , 
# 153 "./pack-test-1.c"
 f5
# 153 "./pack-test-1.c" 3 4
 ) 
# 153 "./pack-test-1.c"
 == 
# 153 "./pack-test-1.c" 3 4
 __builtin_offsetof (
# 153 "./pack-test-1.c"
 struct s10
# 153 "./pack-test-1.c" 3 4
 , 
# 153 "./pack-test-1.c"
 f5
# 153 "./pack-test-1.c" 3 4
 )
# 153 "./pack-test-1.c"
 )) ? 1 : -1];
}
