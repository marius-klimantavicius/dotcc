//type: fn
//options:  -W --c++11
# 0 "./warn/overflow-warn-4.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./warn/overflow-warn-4.C"





# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/limits.h" 1 3 4
# 34 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/limits.h" 3 4
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/syslimits.h" 1 3 4






#pragma GCC diagnostic push
#pragma GCC diagnostic ignored "-Wpedantic"
# 1 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/limits.h" 1 3 4
# 210 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/limits.h" 3 4
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
# 144 "/usr/include/limits.h" 3 4
# 1 "/usr/include/bits/posix1_lim.h" 1 3 4
# 160 "/usr/include/bits/posix1_lim.h" 3 4
# 1 "/usr/include/bits/local_lim.h" 1 3 4
# 38 "/usr/include/bits/local_lim.h" 3 4
# 1 "/usr/include/linux/limits.h" 1 3 4
# 39 "/usr/include/bits/local_lim.h" 2 3 4
# 161 "/usr/include/bits/posix1_lim.h" 2 3 4
# 145 "/usr/include/limits.h" 2 3 4



# 1 "/usr/include/bits/posix2_lim.h" 1 3 4
# 149 "/usr/include/limits.h" 2 3 4



# 1 "/usr/include/bits/xopen_lim.h" 1 3 4
# 33 "/usr/include/bits/xopen_lim.h" 3 4
# 1 "/usr/include/bits/stdio_lim.h" 1 3 4
# 34 "/usr/include/bits/xopen_lim.h" 2 3 4
# 153 "/usr/include/limits.h" 2 3 4
# 211 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/limits.h" 2 3 4
# 10 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/syslimits.h" 2 3 4
#pragma GCC diagnostic pop
# 35 "/mds/gnu/build/gcc-15-20250112/lib/gcc/x86_64-pc-linux-gnu/15.0.0/include/limits.h" 2 3 4
# 7 "./warn/overflow-warn-4.C" 2


# 8 "./warn/overflow-warn-4.C"
enum e {
  E0 = 0x7fffffff
# 9 "./warn/overflow-warn-4.C"
             ,

  E1 = 
# 11 "./warn/overflow-warn-4.C" 3 4
      (0x7fffffff * 2U + 1U) 
# 11 "./warn/overflow-warn-4.C"
               + 1,


  E2 = 2 || 1 / 0,
  E3 = 1 / 0,



  E4 = 0 * (1 / 0),


  E5 = 0x7fffffff 
# 22 "./warn/overflow-warn-4.C"
              + 1,



  E6 = 0 * (0x7fffffff 
# 26 "./warn/overflow-warn-4.C"
                   + 1),



  E7 = (char) 0x7fffffff

# 31 "./warn/overflow-warn-4.C"
};

struct s {
  int a;
  int : 0 * (1 / 0);



  int : 0 * (0x7fffffff 
# 39 "./warn/overflow-warn-4.C"
                    + 1);


};

void
f (void)
{


  int c = 0x7fffffff 
# 49 "./warn/overflow-warn-4.C"
                 + 1;

}


static int sc = 0x7fffffff 
# 54 "./warn/overflow-warn-4.C"
                       + 1;



void *n = 0;



void *p = 0 * (0x7fffffff 
# 62 "./warn/overflow-warn-4.C"
                      + 1);


void *q = 0 * (1 / 0);


void *r = (1 ? 0 : 0x7fffffff 
# 68 "./warn/overflow-warn-4.C"
                         +1);



void
g (int i)
{
  switch (i)
    {
    case 0 * (1/0):



      ;
    case 1 + 0 * (0x7fffffff 
# 82 "./warn/overflow-warn-4.C"
                         + 1):

      ;
    }
}

int
h (void)
{
  return 0x7fffffff 
# 91 "./warn/overflow-warn-4.C"
                + 1;
}

int
h1 (void)
{
  return 0x7fffffff 
# 97 "./warn/overflow-warn-4.C"
                + 1 - 0x7fffffff
# 97 "./warn/overflow-warn-4.C"
                             ;
}

void fuc (unsigned char);
void fsc (signed char);

void
h2 (void)
{
  fsc (0x7f 
# 106 "./warn/overflow-warn-4.C"
                + 1);
  fsc (
# 107 "./warn/overflow-warn-4.C" 3 4
      (-0x7f - 1) 
# 107 "./warn/overflow-warn-4.C"
                - 1);
  fsc (
# 108 "./warn/overflow-warn-4.C" 3 4
      (0x7f * 2 + 1)
# 108 "./warn/overflow-warn-4.C"
               );
  fsc (
# 109 "./warn/overflow-warn-4.C" 3 4
      (0x7f * 2 + 1) 
# 109 "./warn/overflow-warn-4.C"
                + 1);
  fuc (-1);
  fuc (
# 111 "./warn/overflow-warn-4.C" 3 4
      (0x7f * 2 + 1) 
# 111 "./warn/overflow-warn-4.C"
                + 1);
  fuc (
# 112 "./warn/overflow-warn-4.C" 3 4
      (-0x7f - 1)
# 112 "./warn/overflow-warn-4.C"
               );
  fuc (
# 113 "./warn/overflow-warn-4.C" 3 4
      (-0x7f - 1) 
# 113 "./warn/overflow-warn-4.C"
                - 1);
  fuc (-
# 114 "./warn/overflow-warn-4.C" 3 4
       (0x7f * 2 + 1)
# 114 "./warn/overflow-warn-4.C"
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
# 129 "./warn/overflow-warn-4.C"
                        + 1);
  si = (unsigned)0x7fffffff 
# 130 "./warn/overflow-warn-4.C"
                        + 1;
  si = x ? (unsigned)0x7fffffff 
# 131 "./warn/overflow-warn-4.C"
                            + 1 : 1;
  fsi ((unsigned)0x7fffffff 
# 132 "./warn/overflow-warn-4.C"
                        + 2);
  si = (unsigned)0x7fffffff 
# 133 "./warn/overflow-warn-4.C"
                        + 2;
  si = x ? (unsigned)0x7fffffff 
# 134 "./warn/overflow-warn-4.C"
                            + 2 : 1;
  fsi (
# 135 "./warn/overflow-warn-4.C" 3 4
      (0x7fffffff * 2U + 1U)
# 135 "./warn/overflow-warn-4.C"
              );
  si = 
# 136 "./warn/overflow-warn-4.C" 3 4
      (0x7fffffff * 2U + 1U)
# 136 "./warn/overflow-warn-4.C"
              ;
  fui (-1);
  ui = -1;
  ui = x ? -1 : 1U;
  fui (
# 140 "./warn/overflow-warn-4.C" 3 4
      (-0x7fffffff - 1)
# 140 "./warn/overflow-warn-4.C"
             );
  ui = 
# 141 "./warn/overflow-warn-4.C" 3 4
      (-0x7fffffff - 1)
# 141 "./warn/overflow-warn-4.C"
             ;
  ui = x ? 
# 142 "./warn/overflow-warn-4.C" 3 4
          (-0x7fffffff - 1) 
# 142 "./warn/overflow-warn-4.C"
                  : 1U;
}
