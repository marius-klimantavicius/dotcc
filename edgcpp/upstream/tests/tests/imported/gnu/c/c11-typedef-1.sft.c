//type: fn
//options: --c11
# 0 "./c11-typedef-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c11-typedef-1.c"
# 9 "./c11-typedef-1.c"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 34 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 1 3 4






#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 1 3 4
# 210 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 3 4
# 1 "/usr/include/limits.h" 1 3 4
# 26 "/usr/include/limits.h" 3 4
# 1 "/usr/include/features.h" 1 3 4
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
# 27 "/usr/include/limits.h" 2 3 4
# 211 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 10 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/syslimits.h" 2 3 4
#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/limits.h" 2 3 4
# 10 "./c11-typedef-1.c" 2


# 11 "./c11-typedef-1.c"
typedef int TI;
typedef int TI2;
typedef TI2 TI;
typedef TI TI2;

enum e { E1 = 0, E2 = 0x7fffffff
# 16 "./c11-typedef-1.c"
                            , E3 = -1 };
typedef enum e TE;
typedef enum e TE;
typedef int TE;

struct s;
typedef struct s TS;
struct s { int i; };
typedef struct s TS;

typedef int IA[];
typedef TI2 IA[];
typedef int A2[2];
typedef TI A2[2];
typedef IA A2;
typedef int A3[3];
typedef A3 IA;

typedef void F(int);
typedef void F(TI);
typedef void F(enum e);

typedef int G(void);
typedef TI G(void);
typedef enum e G(void);

typedef int *P;
typedef TI *P;
typedef enum e *P;

typedef void F2();
typedef void F2();
typedef void F2(int);

void
f (void)
{
  int a = 1;
  int b = 2;
  typedef void FN(int (*p)[a]);
  typedef void FN(int (*p)[b]);
  typedef void FN(int (*p)[*]);
  typedef void FN(int (*p)[1]);
  typedef void FN2(int (*p)[a]);
  typedef void FN2(int (*p)[b]);
  typedef void FN2(int (*p)[*]);
  typedef void FN2(int (*p)[]);
  typedef int AV[a];
  typedef int AV[b-1];
  typedef int AAa[a];
  typedef int AAb[b-1];
  typedef AAa *VF(void);
  typedef AAb *VF(void);
  typedef AAa AAa;
}
