//type: rp
//options: --c23
# 0 "./c23-bool-1.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c23-bool-1.c"




# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdbool.h" 1 3 4
# 6 "./c23-bool-1.c" 2




extern void abort (void);
extern void exit (int);
extern int strcmp (const char *, const char *);
# 30 "./c23-bool-1.c"
int
main (void)
{
  if (_Generic (true, _Bool : 1) != 1)
    abort ();
  if (true != 1)
    abort ();
  if (_Generic (false, _Bool : 1) != 1)
    abort ();
  if (false != 0)
    abort ();
  if (strcmp ("1", "1") != 0)
    abort ();
  exit (0);
}
