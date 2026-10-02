//type: fn
//options: --c99
# 0 "./overflow-warn-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./overflow-warn-1.c"





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
# 7 "./overflow-warn-1.c" 2


# 8 "./overflow-warn-1.c"
enum e {
  E0 = 0x7fffffff
# 9 "./overflow-warn-1.c"
             ,

  E1 = 
# 11 "./overflow-warn-1.c" 3 4
      (0x7fffffff * 2U + 1U) 
# 11 "./overflow-warn-1.c"
               + 1,


  E2 = 2 || 1 / 0,
  E3 = 1 / 0,



  E4 = 0 * (1 / 0),

  E5 = 0x7fffffff 
# 21 "./overflow-warn-1.c"
              + 1,

  E6 = 0 * (0x7fffffff 
# 23 "./overflow-warn-1.c"
                   + 1),

  E7 = (char) 0x7fffffff

# 26 "./overflow-warn-1.c"
};

struct s {
  int a;
  int : 0 * (1 / 0);

  int : 0 * (0x7fffffff 
# 32 "./overflow-warn-1.c"
                    + 1);
};

void
f (void)
{


  int c = 0x7fffffff 
# 40 "./overflow-warn-1.c"
                 + 1;
}


static int sc = 0x7fffffff 
# 44 "./overflow-warn-1.c"
                       + 1;




void *p = 0 * (0x7fffffff 
# 49 "./overflow-warn-1.c"
                      + 1);

void *q = 0 * (1 / 0);


void *r = (1 ? 0 : 0x7fffffff 
# 54 "./overflow-warn-1.c"
                         +1);

void
g (int i)
{
  switch (i)
    {
    case 0 * (1/0):

      ;
    case 1 + 0 * (0x7fffffff 
# 64 "./overflow-warn-1.c"
                         + 1):
      ;
    }
}

int
h (void)
{
  return 0x7fffffff 
# 72 "./overflow-warn-1.c"
                + 1;
}

int
h1 (void)
{
  return 0x7fffffff 
# 78 "./overflow-warn-1.c"
                + 1 - 0x7fffffff
# 78 "./overflow-warn-1.c"
                             ;
}

void fuc (unsigned char);
void fsc (signed char);

void
h2 (void)
{
  fsc (0x7f 
# 87 "./overflow-warn-1.c"
                + 1);
  fsc (
# 88 "./overflow-warn-1.c" 3 4
      (-0x7f - 1) 
# 88 "./overflow-warn-1.c"
                - 1);
  fsc (
# 89 "./overflow-warn-1.c" 3 4
      (0x7f * 2 + 1)
# 89 "./overflow-warn-1.c"
               );
  fsc (
# 90 "./overflow-warn-1.c" 3 4
      (0x7f * 2 + 1) 
# 90 "./overflow-warn-1.c"
                + 1);
  fuc (-1);
  fuc (
# 92 "./overflow-warn-1.c" 3 4
      (0x7f * 2 + 1) 
# 92 "./overflow-warn-1.c"
                + 1);
  fuc (
# 93 "./overflow-warn-1.c" 3 4
      (-0x7f - 1)
# 93 "./overflow-warn-1.c"
               );
  fuc (
# 94 "./overflow-warn-1.c" 3 4
      (-0x7f - 1) 
# 94 "./overflow-warn-1.c"
                - 1);
  fuc (-
# 95 "./overflow-warn-1.c" 3 4
       (0x7f * 2 + 1)
# 95 "./overflow-warn-1.c"
                );
}

void fui (unsigned int);
void fsi (signed int);

int si;
unsigned ui;

void
h2i (int x)
{



  fsi ((unsigned)0x7fffffff 
# 110 "./overflow-warn-1.c"
                        + 1);
  si = (unsigned)0x7fffffff 
# 111 "./overflow-warn-1.c"
                        + 1;
  si = x ? (unsigned)0x7fffffff 
# 112 "./overflow-warn-1.c"
                            + 1 : 1;
  fsi ((unsigned)0x7fffffff 
# 113 "./overflow-warn-1.c"
                        + 2);
  si = (unsigned)0x7fffffff 
# 114 "./overflow-warn-1.c"
                        + 2;
  si = x ? (unsigned)0x7fffffff 
# 115 "./overflow-warn-1.c"
                            + 2 : 1;
  fsi (
# 116 "./overflow-warn-1.c" 3 4
      (0x7fffffff * 2U + 1U)
# 116 "./overflow-warn-1.c"
              );
  si = 
# 117 "./overflow-warn-1.c" 3 4
      (0x7fffffff * 2U + 1U)
# 117 "./overflow-warn-1.c"
              ;
  fui (-1);
  ui = -1;
  ui = x ? -1 : 1U;
  fui (
# 121 "./overflow-warn-1.c" 3 4
      (-0x7fffffff - 1)
# 121 "./overflow-warn-1.c"
             );
  ui = 
# 122 "./overflow-warn-1.c" 3 4
      (-0x7fffffff - 1)
# 122 "./overflow-warn-1.c"
             ;
  ui = x ? 
# 123 "./overflow-warn-1.c" 3 4
          (-0x7fffffff - 1) 
# 123 "./overflow-warn-1.c"
                  : 1U;
}
