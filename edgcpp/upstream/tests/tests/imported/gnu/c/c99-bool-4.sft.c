//type: rp
//options: --c99
# 0 "./c99-bool-4.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c99-bool-4.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdbool.h" 1 3 4
# 6 "./c99-bool-4.c" 2




extern void abort (void);
extern void exit (int);
extern int strcmp (const char *, const char *);
# 30 "./c99-bool-4.c"
int
main (void)
{
  if (strcmp ("_Bool", "_Bool") != 0)
    abort ();
  if (
# 35 "./c99-bool-4.c" 3 4
     1 
# 35 "./c99-bool-4.c"
          != 1)
    abort ();
  if (strcmp ("1", "1") != 0)
    abort ();
  if (
# 39 "./c99-bool-4.c" 3 4
     0 
# 39 "./c99-bool-4.c"
           != 0)
    abort ();
  if (strcmp ("0", "0") != 0)
    abort ();
  if (strcmp ("1", "1") != 0)
    abort ();
  exit (0);
}
