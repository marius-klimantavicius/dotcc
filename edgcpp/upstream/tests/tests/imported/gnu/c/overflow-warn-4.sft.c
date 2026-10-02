//type: fn
//options: --c99
# 0 "./overflow-warn-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./overflow-warn-4.c"





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
# 7 "./overflow-warn-4.c" 2


# 8 "./overflow-warn-4.c"
enum e {
  E0 = 0x7fffffff
# 9 "./overflow-warn-4.c"
             ,

  E1 = 
# 11 "./overflow-warn-4.c" 3 4
      (0x7fffffff * 2U + 1U) 
# 11 "./overflow-warn-4.c"
               + 1,


  E2 = 2 || 1 / 0,
  E3 = 1 / 0,



  E4 = 0 * (1 / 0),

  E5 = 0x7fffffff 
# 21 "./overflow-warn-4.c"
              + 1,


  E6 = 0 * (0x7fffffff 
# 24 "./overflow-warn-4.c"
                   + 1),


  E7 = (char) 0x7fffffff

# 28 "./overflow-warn-4.c"
};

struct s {
  int a;
  int : 0 * (1 / 0);

  int : 0 * (0x7fffffff 
# 34 "./overflow-warn-4.c"
                    + 1);

};

void
f (void)
{


  int c = 0x7fffffff 
# 43 "./overflow-warn-4.c"
                 + 1;

}


static int sc = 0x7fffffff 
# 48 "./overflow-warn-4.c"
                       + 1;





void *p = 0 * (0x7fffffff 
# 54 "./overflow-warn-4.c"
                      + 1);


void *q = 0 * (1 / 0);


void *r = (1 ? 0 : 0x7fffffff 
# 60 "./overflow-warn-4.c"
                         +1);

void
g (int i)
{
  switch (i)
    {
    case 0 * (1/0):

      ;
    case 1 + 0 * (0x7fffffff 
# 70 "./overflow-warn-4.c"
                         + 1):

      ;
    }
}

int
h (void)
{
  return 0x7fffffff 
# 79 "./overflow-warn-4.c"
                + 1;
}

int
h1 (void)
{
  return 0x7fffffff 
# 85 "./overflow-warn-4.c"
                + 1 - 0x7fffffff
# 85 "./overflow-warn-4.c"
                             ;
}

void fuc (unsigned char);
void fsc (signed char);

void
h2 (void)
{
  fsc (0x7f 
# 94 "./overflow-warn-4.c"
                + 1);
  fsc (
# 95 "./overflow-warn-4.c" 3 4
      (-0x7f - 1) 
# 95 "./overflow-warn-4.c"
                - 1);
  fsc (
# 96 "./overflow-warn-4.c" 3 4
      (0x7f * 2 + 1)
# 96 "./overflow-warn-4.c"
               );
  fsc (
# 97 "./overflow-warn-4.c" 3 4
      (0x7f * 2 + 1) 
# 97 "./overflow-warn-4.c"
                + 1);
  fuc (-1);
  fuc (
# 99 "./overflow-warn-4.c" 3 4
      (0x7f * 2 + 1) 
# 99 "./overflow-warn-4.c"
                + 1);
  fuc (
# 100 "./overflow-warn-4.c" 3 4
      (-0x7f - 1)
# 100 "./overflow-warn-4.c"
               );
  fuc (
# 101 "./overflow-warn-4.c" 3 4
      (-0x7f - 1) 
# 101 "./overflow-warn-4.c"
                - 1);
  fuc (-
# 102 "./overflow-warn-4.c" 3 4
       (0x7f * 2 + 1)
# 102 "./overflow-warn-4.c"
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
# 117 "./overflow-warn-4.c"
                        + 1);
  si = (unsigned)0x7fffffff 
# 118 "./overflow-warn-4.c"
                        + 1;
  si = x ? (unsigned)0x7fffffff 
# 119 "./overflow-warn-4.c"
                            + 1 : 1;
  fsi ((unsigned)0x7fffffff 
# 120 "./overflow-warn-4.c"
                        + 2);
  si = (unsigned)0x7fffffff 
# 121 "./overflow-warn-4.c"
                        + 2;
  si = x ? (unsigned)0x7fffffff 
# 122 "./overflow-warn-4.c"
                            + 2 : 1;
  fsi (
# 123 "./overflow-warn-4.c" 3 4
      (0x7fffffff * 2U + 1U)
# 123 "./overflow-warn-4.c"
              );
  si = 
# 124 "./overflow-warn-4.c" 3 4
      (0x7fffffff * 2U + 1U)
# 124 "./overflow-warn-4.c"
              ;
  fui (-1);
  ui = -1;
  ui = x ? -1 : 1U;
  fui (
# 128 "./overflow-warn-4.c" 3 4
      (-0x7fffffff - 1)
# 128 "./overflow-warn-4.c"
             );
  ui = 
# 129 "./overflow-warn-4.c" 3 4
      (-0x7fffffff - 1)
# 129 "./overflow-warn-4.c"
             ;
  ui = x ? 
# 130 "./overflow-warn-4.c" 3 4
          (-0x7fffffff - 1) 
# 130 "./overflow-warn-4.c"
                  : 1U;
}
