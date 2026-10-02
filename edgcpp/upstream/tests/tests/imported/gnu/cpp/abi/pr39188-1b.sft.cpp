//type: fp
//options: 
# 0 "./abi/pr39188-1b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/pr39188-1b.C"
# 1 "./abi/pr39188-1.h" 1
inline int
f (int x)
{
  static union
    {
      int i;
    };
  int j = i;
  i = x;
  return j;
}
# 2 "./abi/pr39188-1b.C" 2

extern "C" void abort ();

extern int x (int);

int
main (void)
{
  if (x (1) != 0)
    abort ();
  if (f (1) != 1)
    abort ();
  return 0;
}
