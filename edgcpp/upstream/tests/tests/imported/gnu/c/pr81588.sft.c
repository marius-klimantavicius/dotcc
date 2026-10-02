//type: rp
//options: 
# 0 "./pr81588.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./pr81588.c"




long long int a = 5011877430933453486LL, c = 1;
unsigned short b = 24847;

# 1 "./tree-ssa/pr81588.c" 1




extern long long int a, c;
extern unsigned short b;



__attribute__((noinline, noclone)) void
foo (void)
{
  if ((b > a) != (1 + (a < 0)))
    c = 0;
}
# 9 "./pr81588.c" 2

int
main ()
{
  foo ();
  if (c != 0)
    __builtin_abort ();
  a = 24846;
  c = 1;
  foo ();
  if (c != 1)
    __builtin_abort ();
  a = -5;
  foo ();
  if (c != 0)
    __builtin_abort ();
  return 0;
}
