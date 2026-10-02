//type: fp
//options: 
# 0 "./lto/20081222_0.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./lto/20081222_0.c"

# 1 "./lto/20081222_0.h" 1
int x();
# 3 "./lto/20081222_0.c" 2

extern void abort (void);

int
main ()
{
  if (x () == 7)
    return 0;
  abort ();
}
