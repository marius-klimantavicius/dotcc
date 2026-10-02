//type: rp
//options: 
# 0 "./field-merge-12.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./field-merge-12.c"






# 1 "/mds/gnu/build/gcc-15-20250223/lib/gcc/x86_64-pc-linux-gnu/15.0.1/include/stdbool.h" 1 3 4
# 8 "./field-merge-12.c" 2

struct s {
  long long q;
};

struct s x1 = { 1 };
struct s xm1 = { -1 };
struct s x8 = { 8 };
struct s x0 = { 0 };

bool f(struct s *p)
{
  int q = (int)p->q;
  return (q < 0) || (q & 7);
}

int main ()
{
  if (!f (&x1))
    __builtin_abort ();
  if (!f (&xm1))
    __builtin_abort ();
  if (f (&x8))
    __builtin_abort ();
  if (f (&x0))
    __builtin_abort ();
  return 0;
}
