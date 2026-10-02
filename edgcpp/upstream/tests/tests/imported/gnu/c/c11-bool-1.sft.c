//type: rp
//options: --c11
# 0 "./c11-bool-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c11-bool-1.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdbool.h" 1 3 4
# 6 "./c11-bool-1.c" 2




extern void abort (void);
extern void exit (int);
extern int strcmp (const char *, const char *);
# 30 "./c11-bool-1.c"
int
main (void)
{
  if (strcmp ("_Bool", "_Bool") != 0)
    abort ();
  if (_Generic (
# 35 "./c11-bool-1.c" 3 4
               1
# 35 "./c11-bool-1.c"
                   , int : 1) != 1)
    abort ();
  if (
# 37 "./c11-bool-1.c" 3 4
     1 
# 37 "./c11-bool-1.c"
          != 1)
    abort ();
  if (strcmp ("1", "1") != 0)
    abort ();
  if (_Generic (
# 41 "./c11-bool-1.c" 3 4
               0
# 41 "./c11-bool-1.c"
                    , int : 1) != 1)
    abort ();
  if (
# 43 "./c11-bool-1.c" 3 4
     0 
# 43 "./c11-bool-1.c"
           != 0)
    abort ();
  if (strcmp ("0", "0") != 0)
    abort ();
  if (strcmp ("1", "1") != 0)
    abort ();
  exit (0);
}
