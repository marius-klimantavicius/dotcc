//type: fp
//options: 
# 0 "./abi/pr39188-2b.C"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./abi/pr39188-2b.C"
# 1 "./abi/pr39188-2.h" 1
template<typename T>
T
f (T x)
{
  static union
    {
      T i;
    };
  T j = i;
  i = x;
  return j;
}
# 2 "./abi/pr39188-2b.C" 2

extern "C" void abort ();

extern int x (int);

int
main (void)
{
  if (x (1) != 0)
    abort ();
  if (f<int> (1) != 1)
    abort ();
  return 0;
}
