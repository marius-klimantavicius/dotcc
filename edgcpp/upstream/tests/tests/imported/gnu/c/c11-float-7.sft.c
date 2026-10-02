//type: rp
//options: --c11
# 0 "./c11-float-7.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c11-float-7.c"





# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/float.h" 1 3 4
# 7 "./c11-float-7.c" 2

extern void abort (void);
extern void exit (int);

int
main (void)
{
  volatile long double ee = 1.0;
  long double eps = ee;
  while (ee + 1.0 != 1.0)
    {
      eps = ee;
      ee = eps / 2;
    }
  if (eps != 1.08420217248550443400745280086994171e-19L
# 21 "./c11-float-7.c"
                        )
    abort ();
  exit (0);
}
