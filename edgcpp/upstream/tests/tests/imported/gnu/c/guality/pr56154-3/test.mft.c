//source_files: pr56154-aux.c
//type: rp
//options: 
# 0 "./guality/pr56154-3.c"
# 0 "<built-in>"
# 0 "<command-line>"
# 1 "/usr/include/stdc-predef.h" 1 3 4
# 0 "<command-line>" 2
# 1 "./guality/pr56154-3.c"





# 1 "./guality/../nop.h" 1
# 7 "./guality/pr56154-3.c" 2

extern void abort (void);

__attribute__((noinline, noclone)) int
foo (int x)
{
  x++;
  x++;
  x++;
  x++;
  x++;
  x++;
  x++;
  x++;
  asm ("nop" : : : "memory");
  asm ("nop" : : : "memory");
  return x;
}

void
test_main (void)
{
  if (foo (20) != 28)
    abort ();
}
