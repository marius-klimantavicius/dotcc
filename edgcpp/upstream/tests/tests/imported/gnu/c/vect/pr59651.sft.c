//type: fp
//options: 
# 0 "./vect/pr59651.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./vect/pr59651.c"

# 1 "./vect/../torture/pr59651.c" 1



extern void abort (void);
int a[] = { 0, 0, 0, 0, 0, 0, 0, 6 };

int b;
int
main ()
{
  for (;;)
    {
      for (b = 7; b; --b)
 a[b] = a[7] > 1;
      break;
    }
  if (a[1] != 0)
    abort ();
  return 0;
}
# 3 "./vect/pr59651.c" 2
