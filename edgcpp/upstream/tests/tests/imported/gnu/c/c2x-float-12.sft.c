//type: rp
//options: --c23
# 0 "./c2x-float-12.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./c2x-float-12.c"




# 1 "/mds/gnu/build/gcc-13.1.0/lib/gcc/x86_64-pc-linux-gnu/13.1.0/include/float.h" 1 3 4
# 6 "./c2x-float-12.c" 2

extern void abort (void);
extern void exit (int);

int
main (void)
{
  volatile long double x = 1.0L;
  for (int i = 0; i < 64 
# 14 "./c2x-float-12.c"
                                   - 1; i++)
    x /= 2;
  if (x != 1.08420217248550443400745280086994171e-19L
# 16 "./c2x-float-12.c"
                      )
    abort ();
  exit (0);
}
